using LinqSTG;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json.Linq;

namespace LinqSTG.Expression.ToLua
{
    /// <summary>
    /// Describes the kind of Lua binding a node's codegen produces, so that
    /// polymorphic operators (e.g. <see cref="Parser.IntrinsicAdd"/> vs
    /// <see cref="Parser.IntrinsicAddVector2"/>) can pick the right variant.
    /// </summary>
    public enum PortShape
    {
        /// <summary>
        /// The output shape is irrelevant or not consumed by arithmetic
        /// (movements, patterns, transformations, shooters, ...).
        /// </summary>
        Unknown,
        /// <summary> Produces a single <c>__val</c> Lua local. </summary>
        Scalar,
        /// <summary> Produces <c>__valx</c>/<c>__valy</c> Lua locals. </summary>
        Vector2,
    }

    /// <summary>
    /// A <see cref="LuaParser"/> paired with the <see cref="PortShape"/> of the
    /// output it produces, so downstream nodes can dispatch on shape.
    /// </summary>
    public sealed record TypedLuaParser(LuaParser LuaParser, PortShape Shape);

    /// <summary>
    /// Translates LinqSTG blueprint nodes into <see cref="LuaParser"/> delegates.
    /// One instance is constructed per blueprint translation and bound to a
    /// per-blueprint <see cref="Parser"/>, so the generated unique-variable ids
    /// are scoped to a single blueprint rather than the whole process.
    /// </summary>
    public class NodeTranslator
    {
        private readonly Parser _parser;

        public NodeTranslator(Parser parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// The empty <see cref="TypedLuaParser"/> (emits no code). Bound to this
        /// translator's <see cref="Parser"/> instance for consistent scoping.
        /// </summary>
        public TypedLuaParser Empty => new(_parser.Empty(), PortShape.Unknown);

        public TypedLuaParser Translate(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs)
        {
            return node.NodeType switch
            {
                "Shoot" => new TypedLuaParser(_parser.Shoot(
                    ParserOf(node, inputs, "pattern"),
                    ParserOf(node, inputs, "movement")), PortShape.Unknown),

                // The repeat nodes embed MapPattern (an optional per-element
                // transformation); an unconnected mapper degrades to the identity,
                // matching the ViewModel preview's DefaultMapper fallback.
                "RepeatWithIntervalPattern" => new TypedLuaParser(_parser.MapPattern(
                    _parser.RepeatWithIntervalPattern(
                        InputOrConstant(node, inputs, "times").LuaParser,
                        InputOrConstant(node, inputs, "interval").LuaParser,
                        InputOrDefaultRepeater(inputs, "repeater").LuaParser),
                    InputOrEmpty(inputs, "mapper").LuaParser), PortShape.Unknown),

                "RepeatPattern" => new TypedLuaParser(_parser.MapPattern(
                    _parser.RepeatPattern(
                        InputOrConstant(node, inputs, "times").LuaParser,
                        InputOrDefaultRepeater(inputs, "repeater").LuaParser),
                    InputOrEmpty(inputs, "mapper").LuaParser), PortShape.Unknown),

                "RepeaterKey" => new TypedLuaParser(_parser.Repeater(
                    InputOrLiteral(inputs, "id_key", "ID").LuaParser,
                    InputOrLiteral(inputs, "total_key", "Total").LuaParser), PortShape.Unknown),

                "Sample01MinMax" => new TypedLuaParser(_parser.Sample01MinMax(
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser,
                    InputOrConstant(node, inputs, "lower_bound").LuaParser,
                    InputOrConstant(node, inputs, "upper_bound").LuaParser,
                    ReadIntervalType(node, "interval_type")), PortShape.Scalar),

                "Sample01" => new TypedLuaParser(_parser.Sample01(
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser,
                    ReadIntervalType(node, "interval_type")), PortShape.Scalar),

                "MinMax" => new TypedLuaParser(_parser.MinMax(
                    InputOrUnknown(node, inputs, "input_value").LuaParser,
                    InputOrConstant(node, inputs, "lower_bound").LuaParser,
                    InputOrConstant(node, inputs, "upper_bound").LuaParser), PortShape.Scalar),

                "TakeRepeaterFromContext" => new TypedLuaParser(_parser.TakeRepeaterFromContext(
                    InputOrUnknown(node, inputs, "repeater_key").LuaParser), PortShape.Unknown),

                "Vector2FromRotationDistance" => new TypedLuaParser(_parser.VectorFromAngleLength(
                    InputOrConstant(node, inputs, "rotation").LuaParser,
                    InputOrConstant(node, inputs, "distance").LuaParser), PortShape.Vector2),

                "Vector2" => new TypedLuaParser(_parser.Vector2(
                    InputOrConstant(node, inputs, "x").LuaParser,
                    InputOrConstant(node, inputs, "y").LuaParser), PortShape.Vector2),

                // Vector2 with no connection degrades to the zero vector,
                // matching the ViewModel preview's Vector2.Zero fallback.
                "RotateVector" => new TypedLuaParser(_parser.Vector2Rotate(
                    InputOrZeroVector2(inputs, "vector2"),
                    InputOrConstant(node, inputs, "angle").LuaParser), PortShape.Vector2),

                "ConstantFloat" => ConstantFromEditor(node, "value"),

                "ConstantInt" => ConstantFromEditor(node, "value"),

                "ConstantString" => ConstantFromEditor(node, "value"),

                // Variable list references: the name is assumed to be a variable
                // defined in the outer scope (e.g. _infinite), so it is emitted
                // verbatim as the scalar value. Both variants are Lua numbers.
                "PatternVariableFloat" => VariableFromEditor(node, "name"),
                "PatternVariableInt" => VariableFromEditor(node, "name"),

                // The locked built-ins translate to their fixed outer-scope targets.
                "Infinite" => new TypedLuaParser(
                    _parser.TakeVariableFromContext(_parser.ConstantString("_infinite")), PortShape.Scalar),
                // self reads the Shoot-header redirect __self: the raw `self` is
                // rebound to the bullet inside create_and_attach_movement.
                "SelfPosition" => new TypedLuaParser(_parser.OuterPosition("__self"), PortShape.Vector2),
                "PlayerPosition" => new TypedLuaParser(_parser.OuterPosition("player"), PortShape.Vector2),

                "Add" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(_parser.IntrinsicAddVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(_parser.IntrinsicAdd(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Subtract" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(_parser.IntrinsicSubtractVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(_parser.IntrinsicSubtract(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Multiply" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(_parser.IntrinsicMultiplyVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(_parser.IntrinsicMultiply(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Divide" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(_parser.IntrinsicDivideVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(_parser.IntrinsicDivide(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Modulo" => new TypedLuaParser(_parser.IntrinsicModulo(
                    ParserOf(node, inputs, "a"),
                    ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Negate" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(_parser.IntrinsicNegateVector2(
                        ParserOf(node, inputs, "a")), PortShape.Vector2)
                    : new TypedLuaParser(_parser.IntrinsicNegate(
                        ParserOf(node, inputs, "a")), PortShape.Scalar),

                "Sin" => new TypedLuaParser(_parser.Sin(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Cos" => new TypedLuaParser(_parser.Cos(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Tan" => new TypedLuaParser(_parser.Tan(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ASin" => new TypedLuaParser(_parser.ASin(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ACos" => new TypedLuaParser(_parser.ACos(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ATan" => new TypedLuaParser(_parser.ATan(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "DegToRad" => new TypedLuaParser(_parser.DegToRad(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "RadToDeg" => new TypedLuaParser(_parser.RadToDeg(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Abs" => new TypedLuaParser(_parser.Abs(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Sqrt" => new TypedLuaParser(_parser.Sqrt(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Floor" => new TypedLuaParser(_parser.Floor(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Ceil" => new TypedLuaParser(_parser.Ceil(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Sign" => new TypedLuaParser(_parser.Sign(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Exp" => new TypedLuaParser(_parser.Exp(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Log" => new TypedLuaParser(_parser.Log(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ATan2" => new TypedLuaParser(_parser.ATan2(InputOrConstant(node, inputs, "y").LuaParser, InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Pow" => new TypedLuaParser(_parser.Pow(InputOrConstant(node, inputs, "base").LuaParser, InputOrConstant(node, inputs, "exponent").LuaParser), PortShape.Scalar),
                "Min" => new TypedLuaParser(_parser.Min(InputOrConstant(node, inputs, "a").LuaParser, InputOrConstant(node, inputs, "b").LuaParser), PortShape.Scalar),
                "Max" => new TypedLuaParser(_parser.Max(InputOrConstant(node, inputs, "a").LuaParser, InputOrConstant(node, inputs, "b").LuaParser), PortShape.Scalar),
                "Clamp" => new TypedLuaParser(_parser.Clamp(InputOrConstant(node, inputs, "x").LuaParser, InputOrConstant(node, inputs, "min").LuaParser, InputOrConstant(node, inputs, "max").LuaParser), PortShape.Scalar),
                "Lerp" => new TypedLuaParser(_parser.Lerp(InputOrConstant(node, inputs, "a").LuaParser, InputOrConstant(node, inputs, "b").LuaParser, InputOrConstant(node, inputs, "t").LuaParser), PortShape.Scalar),
                "RandomFloat" => new TypedLuaParser(_parser.RandomFloat(
                    InputOrConstant(node, inputs, "start").LuaParser,
                    InputOrConstant(node, inputs, "end").LuaParser), PortShape.Scalar),
                "RandomInt" => new TypedLuaParser(_parser.RandomInt(
                    InputOrConstant(node, inputs, "start").LuaParser,
                    InputOrConstant(node, inputs, "end").LuaParser), PortShape.Scalar),
                "RandomSign" => new TypedLuaParser(_parser.RandomSign(), PortShape.Scalar),

                "FloatToInt" => new TypedLuaParser(_parser.FloatToInt(
                    InputOrUnknown(node, inputs, "float").LuaParser), PortShape.Scalar),

                "IntToFloat" => new TypedLuaParser(_parser.IntToFloat(
                    InputOrUnknown(node, inputs, "int").LuaParser), PortShape.Scalar),

                "UniformVelocityMovement" => new TypedLuaParser(_parser.UniformVelocityMovement(
                    InputOrUnknown(node, inputs, "velocity").LuaParser), PortShape.Unknown),

                "FromPointMovement" => new TypedLuaParser(_parser.FromPointMovement(
                    InputOrUnknown(node, inputs, "position").LuaParser), PortShape.Unknown),

                "UniformAccelerationMovement" => new TypedLuaParser(_parser.UniformAccelerationMovement(
                    InputOrUnknown(node, inputs, "initial_velocity").LuaParser,
                    InputOrUnknown(node, inputs, "acceleration").LuaParser), PortShape.Unknown),

                "MovementSum" => new TypedLuaParser(_parser.MovementSum(
                    InputOrUnknown(node, inputs, "movement1").LuaParser,
                    InputOrUnknown(node, inputs, "movement2").LuaParser), PortShape.Unknown),

                "MovementOffset" => new TypedLuaParser(_parser.MovementOffset(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "offset").LuaParser), PortShape.Unknown),

                "TakeVariableFromContext" => new TypedLuaParser(_parser.TakeVariableFromContext(
                    InputOrConstant(node, inputs, "key").LuaParser), PortShape.Scalar),

                "MovementAfterTime" => new TypedLuaParser(_parser.MovementAfterTime(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrConstant(node, inputs, "switch_time").LuaParser,
                    InputOrUnknown(node, inputs, "after").LuaParser), PortShape.Unknown),

                "MovementRotate" => new TypedLuaParser(_parser.MovementRotate(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrConstant(node, inputs, "angle").LuaParser), PortShape.Unknown),

                "MovementScale" => new TypedLuaParser(_parser.MovementScale(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "scale").LuaParser), PortShape.Unknown),

                "MovementCartesianToPolar" => new TypedLuaParser(_parser.MovementCartesianToPolar(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "center").LuaParser), PortShape.Unknown),

                "MovementPolarToCartesian" => new TypedLuaParser(_parser.MovementPolarToCartesian(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "center").LuaParser), PortShape.Unknown),

                "MovementScaleTime" => new TypedLuaParser(_parser.MovementScaleTime(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrConstant(node, inputs, "factor").LuaParser), PortShape.Unknown),

                "MovementShiftTime" => new TypedLuaParser(_parser.MovementShiftTime(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrConstant(node, inputs, "delta").LuaParser), PortShape.Unknown),

                "MovementTransformInputTime" => new TypedLuaParser(
                    _parser.MovementTransformInputTime(), PortShape.Scalar),

                // Movement with no connection degrades to sampling the zero movement.
                "MovementPredict" => inputs.TryGetValue("movement", out var predictMovement) && predictMovement != null
                    ? new TypedLuaParser(_parser.MovementPredict(
                        predictMovement.LuaParser,
                        InputOrDefaultScalar(inputs, "time", 0f)), PortShape.Vector2)
                    : new TypedLuaParser(_parser.ZeroVector2(), PortShape.Vector2),

                "MapPattern" => new TypedLuaParser(_parser.MapPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "mapper").LuaParser), PortShape.Unknown),

                "ExtrudePattern" => new TypedLuaParser(_parser.ExtrudePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "sub_pattern").LuaParser), PortShape.Unknown),

                "ExtrudeConcatPattern" => new TypedLuaParser(_parser.ExtrudeConcatPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "sub_pattern").LuaParser), PortShape.Unknown),

                "SingleDataPattern" => new TypedLuaParser(_parser.SingleDataPattern(
                    InputOrEmpty(inputs, "transformation").LuaParser), PortShape.Unknown),

                "SingleIntervalPattern" => new TypedLuaParser(_parser.SingleIntervalPattern(
                    InputOrConstant(node, inputs, "interval").LuaParser), PortShape.Unknown),

                "EmptyPattern" => Empty,

                "FilterPattern" => new TypedLuaParser(_parser.FilterPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "predicate").LuaParser), PortShape.Unknown),

                "ConcatPattern" => new TypedLuaParser(_parser.ConcatPattern(
                    InputOrUnknown(node, inputs, "pattern1").LuaParser,
                    InputOrUnknown(node, inputs, "pattern2").LuaParser), PortShape.Unknown),

                "ReversePattern" => new TypedLuaParser(_parser.ReversePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "SkipPattern" => new TypedLuaParser(_parser.SkipPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrConstant(node, inputs, "count").LuaParser), PortShape.Unknown),

                "TakePattern" => new TypedLuaParser(_parser.TakePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrConstant(node, inputs, "count").LuaParser), PortShape.Unknown),

                "SkipWhilePattern" => new TypedLuaParser(_parser.SkipWhilePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "predicate").LuaParser), PortShape.Unknown),

                "TakeWhilePattern" => new TypedLuaParser(_parser.TakeWhilePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "predicate").LuaParser), PortShape.Unknown),

                "TrimStartPattern" => new TypedLuaParser(_parser.TrimStartPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "TrimEndPattern" => new TypedLuaParser(_parser.TrimEndPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "TrimPattern" => new TypedLuaParser(_parser.TrimPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "Assign" => new TypedLuaParser(_parser.Assign(
                    InputOrEmpty(inputs, "transformation").LuaParser,
                    InputOrConstant(node, inputs, "value").LuaParser,
                    InputOrUnknown(node, inputs, "key").LuaParser), PortShape.Unknown),

                // Vector2Assignment aggregates two Assign nodes over one vector
                // input: the value's components are written to the two editor-backed
                // keys (defaults x/y); an unconnected value degrades to the zero
                // vector, matching the ViewModel preview's Vector2.Zero fallback.
                "Vector2Assignment" => new TypedLuaParser(_parser.AssignVector2(
                    InputOrEmpty(inputs, "transformation").LuaParser,
                    InputOrZeroVector2(inputs, "value"),
                    InputOrConstant(node, inputs, "key_x").LuaParser,
                    InputOrConstant(node, inputs, "key_y").LuaParser), PortShape.Unknown),

                _ => Unknown(node)
            };
        }

        /// <summary>
        /// Resolves the <see cref="TypedLuaParser"/> for a specific output port of a node.
        /// Single-output nodes delegate to <see cref="Translate"/> (ignoring <paramref name="portName"/>);
        /// only multi-output nodes (e.g. <c>Vector2Split</c> with distinct x/y ports) dispatch on it.
        /// </summary>
        public TypedLuaParser TranslateOutput(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            return node.NodeType switch
            {
                "Vector2Split" => portName switch
                {
                    "x" => new TypedLuaParser(_parser.Vector2SplitX(
                        InputOrUnknown(node, inputs, "vector2").LuaParser), PortShape.Scalar),
                    "y" => new TypedLuaParser(_parser.Vector2SplitY(
                        InputOrUnknown(node, inputs, "vector2").LuaParser), PortShape.Scalar),
                    _ => Unknown(node, portName)
                },
                // The variable node's name output passes the resolved key through
                // (string ports carry bare Lua text), while the value output binds
                // the same key via TakeVariableFromContext like the standalone node.
                "Variable" => portName switch
                {
                    "key" => InputOrConstant(node, inputs, "key"),
                    "value" => new TypedLuaParser(_parser.TakeVariableFromContext(
                        InputOrConstant(node, inputs, "key").LuaParser), PortShape.Scalar),
                    _ => Unknown(node, portName)
                },
                // The vector2 variable node aggregates two Variable nodes: the
                // key_x/key_y outputs pass the resolved names through as bare Lua
                // text, the x/y outputs bind them via TakeVariableFromContext, and
                // the vector2 output reads both outer-scope variables directly into
                // the __valx/__valy two-local form.
                "Vector2Variable" => portName switch
                {
                    "key_x" => InputOrConstant(node, inputs, "key_x"),
                    "key_y" => InputOrConstant(node, inputs, "key_y"),
                    "x" => new TypedLuaParser(_parser.TakeVariableFromContext(
                        InputOrConstant(node, inputs, "key_x").LuaParser), PortShape.Scalar),
                    "y" => new TypedLuaParser(_parser.TakeVariableFromContext(
                        InputOrConstant(node, inputs, "key_y").LuaParser), PortShape.Scalar),
                    "vector2" => new TypedLuaParser(_parser.Vector2FromVariables(
                        InputOrConstant(node, inputs, "key_x").LuaParser,
                        InputOrConstant(node, inputs, "key_y").LuaParser), PortShape.Vector2),
                    _ => Unknown(node, portName)
                },
                // The angle/length node's x/y ports expose single components as
                // scalars, reusing the split primitives over the same vector body;
                // the vector2 port keeps the __valx/__valy two-local form.
                "Vector2FromRotationDistance" => portName switch
                {
                    "x" => new TypedLuaParser(_parser.Vector2SplitX(
                        AngleLengthVector(node, inputs)), PortShape.Scalar),
                    "y" => new TypedLuaParser(_parser.Vector2SplitY(
                        AngleLengthVector(node, inputs)), PortShape.Scalar),
                    _ => Translate(node, inputs)
                },
                _ => Translate(node, inputs)
            };
        }

        /// <summary>
        /// The vector body shared by all <c>Vector2FromRotationDistance</c> output
        /// ports: rotation/distance inputs (connected wire or editor constant)
        /// resolved into the <c>__valx</c>/<c>__valy</c> two-local form.
        /// </summary>
        private LuaParser AngleLengthVector(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs)
        {
            return _parser.VectorFromAngleLength(
                InputOrConstant(node, inputs, "rotation").LuaParser,
                InputOrConstant(node, inputs, "distance").LuaParser);
        }

        public TypedLuaParser Unknown(NodeModel node, string? portName = null)
        {
            var nodeType = node.NodeType;
            var suffix = portName != null ? $", port: {portName}" : string.Empty;
            return new TypedLuaParser(_ => new[] { new LuaCodeLine($"--[[ UNKNOWN NODE: {nodeType}{suffix} ]]", 0) }, PortShape.Unknown);
        }

        private LuaParser ParserOf(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed.LuaParser;
            }
            return Unknown(node, portName).LuaParser;
        }

        private PortShape InputShapeOr(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed.Shape;
            }
            return PortShape.Unknown;
        }

        private TypedLuaParser InputOrUnknown(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return Unknown(node, portName);
        }

        private TypedLuaParser InputOrConstant(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string key)
        {
            if (inputs.TryGetValue(key, out var typed) && typed != null)
            {
                return typed;
            }
            return ConstantFromEditor(node, key);
        }

        /// <summary>
        /// Resolves a scalar port to a connected parser, or falls back to a constant.
        /// Used for wire-first ports (e.g. <c>MovementPredict.time</c>) that carry no editor.
        /// </summary>
        private LuaParser InputOrDefaultScalar(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName, float fallback)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed.LuaParser;
            }
            return _parser.ConstantFloat(fallback);
        }

        /// <summary>
        /// Resolves a Vector2 port to a connected parser, or falls back to the
        /// zero vector (e.g. <c>RotateVector.vector2</c> with no wire).
        /// </summary>
        private LuaParser InputOrZeroVector2(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed.LuaParser;
            }
            return _parser.ZeroVector2();
        }

        private TypedLuaParser InputOrDefaultRepeater(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return new TypedLuaParser(_parser.DefaultRepeater(), PortShape.Unknown);
        }

        /// <summary>
        /// Resolves a string port to a connected parser, or falls back to
        /// <paramref name="fallback"/> emitted as a bare Lua identifier (via
        /// <see cref="Parser.ConstantString"/>, which emits raw text without a
        /// <c>local __val =</c> wrapper). Used for repeater keys and other
        /// inputs that must produce a bare identifier rather than a value.
        /// </summary>
        private TypedLuaParser InputOrLiteral(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName, string fallback)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return new TypedLuaParser(_parser.ConstantString(fallback), PortShape.Unknown);
        }

        private TypedLuaParser InputOrEmpty(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return Empty;
        }

        private TypedLuaParser ConstantFromEditor(NodeModel node, string key)
        {
            if (node.Editors.TryGetValue(key, out var token) && token != null)
            {
                var parser = token.Type switch
                {
                    JTokenType.Integer => _parser.ConstantFloat(token.ToObject<float>()),
                    JTokenType.Float => _parser.ConstantFloat(token.ToObject<float>()),
                    JTokenType.String => token.ToObject<string>() is { } s
                        ? _parser.ConstantString(s)
                        : Unknown(node, key).LuaParser,
                    _ => Unknown(node, key).LuaParser
                };
                // Editors only carry scalar values (float/int) or strings; a numeric
                // editor value feeds the scalar __val convention.
                var shape = token.Type == JTokenType.String ? PortShape.Unknown : PortShape.Scalar;
                return new TypedLuaParser(parser, shape);
            }
            return Unknown(node, key);
        }

        private IntervalType ReadIntervalType(NodeModel node, string key)
        {
            if (node.Editors.TryGetValue(key, out var token) && token != null && token.Type == JTokenType.Integer)
            {
                return (IntervalType)token.ToObject<int>();
            }
            return IntervalType.HeadClosed;
        }

        /// <summary>
        /// Resolves a <c>PatternVariableFloat</c>/<c>PatternVariableInt</c> node's
        /// "name" editor into a scalar parser that reads the outer-scope Lua
        /// variable of that name (<c>local __val = &lt;name&gt;</c>), matching the
        /// preview's behavior of looking the name up in the variable list.
        /// </summary>
        private TypedLuaParser VariableFromEditor(NodeModel node, string key)
        {
            if (node.Editors.TryGetValue(key, out var token) && token != null
                && token.Type == JTokenType.String
                && token.ToObject<string>() is { Length: > 0 } name)
            {
                return new TypedLuaParser(_parser.TakeVariableFromContext(_parser.ConstantString(name)), PortShape.Scalar);
            }
            return Unknown(node, key);
        }
    }
}
