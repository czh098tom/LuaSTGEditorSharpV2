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

                "RepeatWithIntervalPattern" => new TypedLuaParser(_parser.RepeatWithIntervalPattern(
                    InputOrConstant(node, inputs, "times").LuaParser,
                    InputOrConstant(node, inputs, "interval").LuaParser,
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser), PortShape.Unknown),

                "RepeatPattern" => new TypedLuaParser(_parser.RepeatPattern(
                    InputOrConstant(node, inputs, "times").LuaParser,
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser), PortShape.Unknown),

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

                "ConstantFloat" => ConstantFromEditor(node, "value"),

                "ConstantInt" => ConstantFromEditor(node, "value"),

                "ConstantString" => ConstantFromEditor(node, "value"),

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

                "FloatToInt" => new TypedLuaParser(_parser.FloatToInt(
                    InputOrUnknown(node, inputs, "float").LuaParser), PortShape.Scalar),

                "IntToFloat" => new TypedLuaParser(_parser.IntToFloat(
                    InputOrUnknown(node, inputs, "int").LuaParser), PortShape.Scalar),

                "UniformVelocityMovement" => new TypedLuaParser(_parser.UniformVelocityMovement(
                    InputOrUnknown(node, inputs, "velocity").LuaParser), PortShape.Unknown),

                "StationaryMovement" => new TypedLuaParser(_parser.StationaryMovement(
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

                "MovementMap" => new TypedLuaParser(_parser.MovementMap(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "transform").LuaParser), PortShape.Unknown),

                "MovementTransformInputPoint" => new TypedLuaParser(
                    _parser.MovementTransformInputPoint(), PortShape.Vector2),

                "MovementTransformFromPoint" => new TypedLuaParser(_parser.MovementTransformFromPoint(
                    InputOrUnknown(node, inputs, "point").LuaParser), PortShape.Unknown),

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
                _ => Translate(node, inputs)
            };
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
    }
}
