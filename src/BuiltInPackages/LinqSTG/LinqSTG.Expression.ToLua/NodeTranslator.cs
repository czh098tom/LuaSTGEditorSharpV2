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
    public sealed record TypedLuaParser(LuaParser LuaParser, PortShape Shape)
    {
        public static readonly TypedLuaParser Empty =
            new(Parser.Empty(), PortShape.Unknown);
    }

    public static class NodeTranslator
    {
        public static TypedLuaParser Translate(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs)
        {
            return node.NodeType switch
            {
                "Shoot" => new TypedLuaParser(Parser.Shoot(
                    ParserOf(node, inputs, "pattern"),
                    ParserOf(node, inputs, "movement")), PortShape.Unknown),

                "RepeatWithIntervalPattern" => new TypedLuaParser(Parser.RepeatWithIntervalPattern(
                    InputOrConstant(node, inputs, "times").LuaParser,
                    InputOrConstant(node, inputs, "interval").LuaParser,
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser), PortShape.Unknown),

                "RepeatPattern" => new TypedLuaParser(Parser.RepeatPattern(
                    InputOrConstant(node, inputs, "times").LuaParser,
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser), PortShape.Unknown),

                "RepeaterKey" => new TypedLuaParser(Parser.Repeater(
                    InputOrLiteral(inputs, "id_key", "ID").LuaParser,
                    InputOrLiteral(inputs, "total_key", "Total").LuaParser), PortShape.Unknown),

                "Sample01MinMax" => new TypedLuaParser(Parser.Sample01MinMax(
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser,
                    InputOrConstant(node, inputs, "lower_bound").LuaParser,
                    InputOrConstant(node, inputs, "upper_bound").LuaParser,
                    ReadIntervalType(node, "interval_type")), PortShape.Scalar),

                "Sample01" => new TypedLuaParser(Parser.Sample01(
                    InputOrDefaultRepeater(inputs, "repeater").LuaParser,
                    ReadIntervalType(node, "interval_type")), PortShape.Scalar),

                "MinMax" => new TypedLuaParser(Parser.MinMax(
                    InputOrUnknown(node, inputs, "input_value").LuaParser,
                    InputOrConstant(node, inputs, "lower_bound").LuaParser,
                    InputOrConstant(node, inputs, "upper_bound").LuaParser), PortShape.Scalar),

                "TakeRepeaterFromContext" => new TypedLuaParser(Parser.TakeRepeaterFromContext(
                    InputOrUnknown(node, inputs, "repeater_key").LuaParser), PortShape.Unknown),

                "Vector2FromRotationDistance" => new TypedLuaParser(Parser.VectorFromAngleLength(
                    InputOrConstant(node, inputs, "rotation").LuaParser,
                    InputOrConstant(node, inputs, "distance").LuaParser), PortShape.Vector2),

                "Vector2" => new TypedLuaParser(Parser.Vector2(
                    InputOrConstant(node, inputs, "x").LuaParser,
                    InputOrConstant(node, inputs, "y").LuaParser), PortShape.Vector2),

                "ConstantFloat" => ConstantFromEditor(node, "value"),

                "ConstantInt" => ConstantFromEditor(node, "value"),

                "ConstantString" => ConstantFromEditor(node, "value"),

                "Add" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(Parser.IntrinsicAddVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(Parser.IntrinsicAdd(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Subtract" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(Parser.IntrinsicSubtractVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(Parser.IntrinsicSubtract(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Multiply" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(Parser.IntrinsicMultiplyVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(Parser.IntrinsicMultiply(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Divide" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(Parser.IntrinsicDivideVector2(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Vector2)
                    : new TypedLuaParser(Parser.IntrinsicDivide(
                        ParserOf(node, inputs, "a"),
                        ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Modulo" => new TypedLuaParser(Parser.IntrinsicModulo(
                    ParserOf(node, inputs, "a"),
                    ParserOf(node, inputs, "b")), PortShape.Scalar),

                "Negate" => InputShapeOr(inputs, "a") == PortShape.Vector2
                    ? new TypedLuaParser(Parser.IntrinsicNegateVector2(
                        ParserOf(node, inputs, "a")), PortShape.Vector2)
                    : new TypedLuaParser(Parser.IntrinsicNegate(
                        ParserOf(node, inputs, "a")), PortShape.Scalar),

                "Sin" => new TypedLuaParser(Parser.Sin(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Cos" => new TypedLuaParser(Parser.Cos(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Tan" => new TypedLuaParser(Parser.Tan(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ASin" => new TypedLuaParser(Parser.ASin(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ACos" => new TypedLuaParser(Parser.ACos(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ATan" => new TypedLuaParser(Parser.ATan(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "DegToRad" => new TypedLuaParser(Parser.DegToRad(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "RadToDeg" => new TypedLuaParser(Parser.RadToDeg(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Abs" => new TypedLuaParser(Parser.Abs(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Sqrt" => new TypedLuaParser(Parser.Sqrt(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Floor" => new TypedLuaParser(Parser.Floor(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Ceil" => new TypedLuaParser(Parser.Ceil(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Sign" => new TypedLuaParser(Parser.Sign(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Exp" => new TypedLuaParser(Parser.Exp(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Log" => new TypedLuaParser(Parser.Log(InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "ATan2" => new TypedLuaParser(Parser.ATan2(InputOrConstant(node, inputs, "y").LuaParser, InputOrConstant(node, inputs, "x").LuaParser), PortShape.Scalar),
                "Pow" => new TypedLuaParser(Parser.Pow(InputOrConstant(node, inputs, "base").LuaParser, InputOrConstant(node, inputs, "exponent").LuaParser), PortShape.Scalar),
                "Min" => new TypedLuaParser(Parser.Min(InputOrConstant(node, inputs, "a").LuaParser, InputOrConstant(node, inputs, "b").LuaParser), PortShape.Scalar),
                "Max" => new TypedLuaParser(Parser.Max(InputOrConstant(node, inputs, "a").LuaParser, InputOrConstant(node, inputs, "b").LuaParser), PortShape.Scalar),
                "Clamp" => new TypedLuaParser(Parser.Clamp(InputOrConstant(node, inputs, "x").LuaParser, InputOrConstant(node, inputs, "min").LuaParser, InputOrConstant(node, inputs, "max").LuaParser), PortShape.Scalar),
                "Lerp" => new TypedLuaParser(Parser.Lerp(InputOrConstant(node, inputs, "a").LuaParser, InputOrConstant(node, inputs, "b").LuaParser, InputOrConstant(node, inputs, "t").LuaParser), PortShape.Scalar),

                "FloatToInt" => new TypedLuaParser(Parser.FloatToInt(
                    InputOrUnknown(node, inputs, "float").LuaParser), PortShape.Scalar),

                "IntToFloat" => new TypedLuaParser(Parser.IntToFloat(
                    InputOrUnknown(node, inputs, "int").LuaParser), PortShape.Scalar),

                "UniformVelocityMovement" => new TypedLuaParser(Parser.UniformVelocityMovement(
                    InputOrUnknown(node, inputs, "velocity").LuaParser), PortShape.Unknown),

                "StationaryMovement" => new TypedLuaParser(Parser.StationaryMovement(
                    InputOrUnknown(node, inputs, "position").LuaParser), PortShape.Unknown),

                "UniformAccelerationMovement" => new TypedLuaParser(Parser.UniformAccelerationMovement(
                    InputOrUnknown(node, inputs, "initial_velocity").LuaParser,
                    InputOrUnknown(node, inputs, "acceleration").LuaParser), PortShape.Unknown),

                "MovementSum" => new TypedLuaParser(Parser.MovementSum(
                    InputOrUnknown(node, inputs, "movement1").LuaParser,
                    InputOrUnknown(node, inputs, "movement2").LuaParser), PortShape.Unknown),

                "MovementOffset" => new TypedLuaParser(Parser.MovementOffset(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "offset").LuaParser), PortShape.Unknown),

                "TakeVariableFromContext" => new TypedLuaParser(Parser.TakeVariableFromContext(
                    InputOrConstant(node, inputs, "key").LuaParser), PortShape.Scalar),

                "MovementAfterTime" => new TypedLuaParser(Parser.MovementAfterTime(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrConstant(node, inputs, "switch_time").LuaParser,
                    InputOrUnknown(node, inputs, "after").LuaParser), PortShape.Unknown),

                "MovementRotate" => new TypedLuaParser(Parser.MovementRotate(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrConstant(node, inputs, "angle").LuaParser), PortShape.Unknown),

                "MovementScale" => new TypedLuaParser(Parser.MovementScale(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "scale").LuaParser), PortShape.Unknown),

                "MovementCartesianToPolar" => new TypedLuaParser(Parser.MovementCartesianToPolar(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "center").LuaParser), PortShape.Unknown),

                "MovementPolarToCartesian" => new TypedLuaParser(Parser.MovementPolarToCartesian(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "center").LuaParser), PortShape.Unknown),

                "MovementMap" => new TypedLuaParser(Parser.MovementMap(
                    InputOrUnknown(node, inputs, "movement").LuaParser,
                    InputOrUnknown(node, inputs, "transform").LuaParser), PortShape.Unknown),

                "MovementTransformInputPoint" => new TypedLuaParser(
                    Parser.MovementTransformInputPoint(), PortShape.Vector2),

                "MovementTransformFromPoint" => new TypedLuaParser(Parser.MovementTransformFromPoint(
                    InputOrUnknown(node, inputs, "point").LuaParser), PortShape.Unknown),

                "MapPattern" => new TypedLuaParser(Parser.MapPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "mapper").LuaParser), PortShape.Unknown),

                "ExtrudePattern" => new TypedLuaParser(Parser.ExtrudePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "sub_pattern").LuaParser), PortShape.Unknown),

                "ExtrudeConcatPattern" => new TypedLuaParser(Parser.ExtrudeConcatPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "sub_pattern").LuaParser), PortShape.Unknown),

                "SingleDataPattern" => new TypedLuaParser(Parser.SingleDataPattern(
                    InputOrEmpty(inputs, "transformation").LuaParser), PortShape.Unknown),

                "SingleIntervalPattern" => new TypedLuaParser(Parser.SingleIntervalPattern(
                    InputOrConstant(node, inputs, "interval").LuaParser), PortShape.Unknown),

                "EmptyPattern" => TypedLuaParser.Empty,

                "FilterPattern" => new TypedLuaParser(Parser.FilterPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "predicate").LuaParser), PortShape.Unknown),

                "ConcatPattern" => new TypedLuaParser(Parser.ConcatPattern(
                    InputOrUnknown(node, inputs, "pattern1").LuaParser,
                    InputOrUnknown(node, inputs, "pattern2").LuaParser), PortShape.Unknown),

                "ReversePattern" => new TypedLuaParser(Parser.ReversePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "SkipPattern" => new TypedLuaParser(Parser.SkipPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrConstant(node, inputs, "count").LuaParser), PortShape.Unknown),

                "TakePattern" => new TypedLuaParser(Parser.TakePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrConstant(node, inputs, "count").LuaParser), PortShape.Unknown),

                "SkipWhilePattern" => new TypedLuaParser(Parser.SkipWhilePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "predicate").LuaParser), PortShape.Unknown),

                "TakeWhilePattern" => new TypedLuaParser(Parser.TakeWhilePattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser,
                    InputOrUnknown(node, inputs, "predicate").LuaParser), PortShape.Unknown),

                "TrimStartPattern" => new TypedLuaParser(Parser.TrimStartPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "TrimEndPattern" => new TypedLuaParser(Parser.TrimEndPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "TrimPattern" => new TypedLuaParser(Parser.TrimPattern(
                    InputOrUnknown(node, inputs, "pattern").LuaParser), PortShape.Unknown),

                "Assign" => new TypedLuaParser(Parser.Assign(
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
        public static TypedLuaParser TranslateOutput(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            return node.NodeType switch
            {
                "Vector2Split" => portName switch
                {
                    "x" => new TypedLuaParser(Parser.Vector2SplitX(
                        InputOrUnknown(node, inputs, "vector2").LuaParser), PortShape.Scalar),
                    "y" => new TypedLuaParser(Parser.Vector2SplitY(
                        InputOrUnknown(node, inputs, "vector2").LuaParser), PortShape.Scalar),
                    _ => Unknown(node, portName)
                },
                _ => Translate(node, inputs)
            };
        }

        public static TypedLuaParser Unknown(NodeModel node, string? portName = null)
        {
            var nodeType = node.NodeType;
            var suffix = portName != null ? $", port: {portName}" : string.Empty;
            return new TypedLuaParser(_ => new[] { new LuaCodeLine($"--[[ UNKNOWN NODE: {nodeType}{suffix} ]]", 0) }, PortShape.Unknown);
        }

        private static LuaParser ParserOf(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed.LuaParser;
            }
            return Unknown(node, portName).LuaParser;
        }

        private static PortShape InputShapeOr(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed.Shape;
            }
            return PortShape.Unknown;
        }

        private static TypedLuaParser InputOrUnknown(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return Unknown(node, portName);
        }

        private static TypedLuaParser InputOrConstant(NodeModel node, IReadOnlyDictionary<string, TypedLuaParser> inputs, string key)
        {
            if (inputs.TryGetValue(key, out var typed) && typed != null)
            {
                return typed;
            }
            return ConstantFromEditor(node, key);
        }

        private static TypedLuaParser InputOrDefaultRepeater(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return new TypedLuaParser(Parser.DefaultRepeater(), PortShape.Unknown);
        }

        /// <summary>
        /// Resolves a string port to a connected parser, or falls back to
        /// <paramref name="fallback"/> emitted as a bare Lua identifier (via
        /// <see cref="Parser.ConstantString"/>, which emits raw text without a
        /// <c>local __val =</c> wrapper). Used for repeater keys and other
        /// inputs that must produce a bare identifier rather than a value.
        /// </summary>
        private static TypedLuaParser InputOrLiteral(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName, string fallback)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return new TypedLuaParser(Parser.ConstantString(fallback), PortShape.Unknown);
        }

        private static TypedLuaParser InputOrEmpty(IReadOnlyDictionary<string, TypedLuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var typed) && typed != null)
            {
                return typed;
            }
            return TypedLuaParser.Empty;
        }

        private static TypedLuaParser ConstantFromEditor(NodeModel node, string key)
        {
            if (node.Editors.TryGetValue(key, out var token) && token != null)
            {
                var parser = token.Type switch
                {
                    JTokenType.Integer => Parser.ConstantFloat(token.ToObject<float>()),
                    JTokenType.Float => Parser.ConstantFloat(token.ToObject<float>()),
                    JTokenType.String => token.ToObject<string>() is { } s
                        ? Parser.ConstantString(s)
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

        private static IntervalType ReadIntervalType(NodeModel node, string key)
        {
            if (node.Editors.TryGetValue(key, out var token) && token != null && token.Type == JTokenType.Integer)
            {
                return (IntervalType)token.ToObject<int>();
            }
            return IntervalType.HeadClosed;
        }
    }
}
