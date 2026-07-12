using LinqSTG;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json.Linq;

namespace LinqSTG.Expression.ToLua
{
    public static class NodeTranslator
    {
        public static LuaParser Translate(NodeModel node, IReadOnlyDictionary<string, LuaParser> inputs)
        {
            return node.NodeType switch
            {
                "Shoot" => Parser.Shoot(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrUnknown(node, inputs, "movement")),

                "RepeatWithIntervalPattern" => Parser.RepeatWithIntervalPattern(
                    InputOrConstant(node, inputs, "times"),
                    InputOrConstant(node, inputs, "interval"),
                    InputOrDefaultRepeater(inputs, "repeater")),

                "RepeatPattern" => Parser.RepeatPattern(
                    InputOrConstant(node, inputs, "times"),
                    InputOrDefaultRepeater(inputs, "repeater")),

                "RepeaterKey" => Parser.TakeRepeaterFromContext(Parser.DefaultRepeater()),

                "Sample01MinMax" => Parser.Sample01MinMax(
                    InputOrDefaultRepeater(inputs, "repeater"),
                    InputOrConstant(node, inputs, "lower_bound"),
                    InputOrConstant(node, inputs, "upper_bound"),
                    ReadIntervalType(node, "interval_type")),

                "Sample01" => Parser.Sample01(
                    InputOrDefaultRepeater(inputs, "repeater"),
                    ReadIntervalType(node, "interval_type")),

                "MinMax" => Parser.MinMax(
                    InputOrUnknown(node, inputs, "input_value"),
                    InputOrConstant(node, inputs, "lower_bound"),
                    InputOrConstant(node, inputs, "upper_bound")),

                "TakeRepeaterFromContext" => Parser.TakeRepeaterFromContext(
                    InputOrUnknown(node, inputs, "repeater_key")),

                "Vector2FromRotationDistance" => Parser.VectorFromAngleLength(
                    InputOrConstant(node, inputs, "rotation"),
                    InputOrConstant(node, inputs, "distance")),

                "Vector2" => Parser.Vector2(
                    InputOrConstant(node, inputs, "x"),
                    InputOrConstant(node, inputs, "y")),

                "ConstantFloat" => ConstantFromEditor(node, "value"),

                "ConstantInt" => ConstantFromEditor(node, "value"),

                "ConstantString" => ConstantFromEditor(node, "value"),

                "Add" => Parser.IntrinsicAdd(
                    InputOrUnknown(node, inputs, "a"),
                    InputOrUnknown(node, inputs, "b")),

                "FloatToInt" => Parser.FloatToInt(
                    InputOrUnknown(node, inputs, "float")),

                "IntToFloat" => Parser.IntToFloat(
                    InputOrUnknown(node, inputs, "int")),

                "UniformVelocityMovement" => Parser.UniformVelocityMovement(
                    InputOrUnknown(node, inputs, "velocity")),

                "StationaryMovement" => Parser.StationaryMovement(
                    InputOrUnknown(node, inputs, "position")),

                "UniformAccelerationMovement" => Parser.UniformAccelerationMovement(
                    InputOrUnknown(node, inputs, "initial_velocity"),
                    InputOrUnknown(node, inputs, "acceleration")),

                "MovementSum" => Parser.MovementSum(
                    InputOrUnknown(node, inputs, "movement1"),
                    InputOrUnknown(node, inputs, "movement2")),

                "MovementOffset" => Parser.MovementOffset(
                    InputOrUnknown(node, inputs, "movement"),
                    InputOrUnknown(node, inputs, "offset")),

                "TakeVariableFromContext" => Parser.TakeVariableFromContext(
                    InputOrConstant(node, inputs, "key")),

                "MovementAfterTime" => Parser.MovementAfterTime(
                    InputOrUnknown(node, inputs, "movement"),
                    InputOrConstant(node, inputs, "switch_time"),
                    InputOrUnknown(node, inputs, "after")),

                "MapPattern" => Parser.MapPattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrUnknown(node, inputs, "mapper")),

                "ExtrudePattern" => Parser.ExtrudePattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrUnknown(node, inputs, "sub_pattern")),

                "ExtrudeConcatPattern" => Parser.ExtrudeConcatPattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrUnknown(node, inputs, "sub_pattern")),

                "SingleDataPattern" => Parser.SingleDataPattern(
                    InputOrEmpty(inputs, "transformation")),

                "SingleIntervalPattern" => Parser.SingleIntervalPattern(
                    InputOrConstant(node, inputs, "interval")),

                "EmptyPattern" => Parser.Empty(),

                "FilterPattern" => Parser.FilterPattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrUnknown(node, inputs, "predicate")),

                "ConcatPattern" => Parser.ConcatPattern(
                    InputOrUnknown(node, inputs, "pattern1"),
                    InputOrUnknown(node, inputs, "pattern2")),

                "ReversePattern" => Parser.ReversePattern(
                    InputOrUnknown(node, inputs, "pattern")),

                "SkipPattern" => Parser.SkipPattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrConstant(node, inputs, "count")),

                "TakePattern" => Parser.TakePattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrConstant(node, inputs, "count")),

                "SkipWhilePattern" => Parser.SkipWhilePattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrUnknown(node, inputs, "predicate")),

                "TakeWhilePattern" => Parser.TakeWhilePattern(
                    InputOrUnknown(node, inputs, "pattern"),
                    InputOrUnknown(node, inputs, "predicate")),

                "TrimStartPattern" => Parser.TrimStartPattern(
                    InputOrUnknown(node, inputs, "pattern")),

                "TrimEndPattern" => Parser.TrimEndPattern(
                    InputOrUnknown(node, inputs, "pattern")),

                "TrimPattern" => Parser.TrimPattern(
                    InputOrUnknown(node, inputs, "pattern")),

                "Assign" => Parser.Assign(
                    InputOrEmpty(inputs, "transformation"),
                    InputOrConstant(node, inputs, "value"),
                    InputOrUnknown(node, inputs, "key")),

                _ => Unknown(node)
            };
        }

        public static LuaParser Unknown(NodeModel node, string? portName = null)
        {
            var nodeType = node.NodeType;
            var suffix = portName != null ? $", port: {portName}" : string.Empty;
            return _ => new[] { new LuaCodeLine($"--[[ UNKNOWN NODE: {nodeType}{suffix} ]]", 0) };
        }

        private static LuaParser InputOrUnknown(NodeModel node, IReadOnlyDictionary<string, LuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var parser) && parser != null)
            {
                return parser;
            }
            return Unknown(node, portName);
        }

        private static LuaParser InputOrConstant(NodeModel node, IReadOnlyDictionary<string, LuaParser> inputs, string key)
        {
            if (inputs.TryGetValue(key, out var parser) && parser != null)
            {
                return parser;
            }
            return ConstantFromEditor(node, key);
        }

        private static LuaParser InputOrDefaultRepeater(IReadOnlyDictionary<string, LuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var parser) && parser != null)
            {
                return parser;
            }
            return Parser.DefaultRepeater();
        }

        private static LuaParser InputOrEmpty(IReadOnlyDictionary<string, LuaParser> inputs, string portName)
        {
            if (inputs.TryGetValue(portName, out var parser) && parser != null)
            {
                return parser;
            }
            return Parser.Empty();
        }

        private static LuaParser ConstantFromEditor(NodeModel node, string key)
        {
            if (node.Editors.TryGetValue(key, out var token) && token != null)
            {
                return token.Type switch
                {
                    JTokenType.Integer => Parser.ConstantFloat(token.ToObject<float>()),
                    JTokenType.Float => Parser.ConstantFloat(token.ToObject<float>()),
                    JTokenType.String => token.ToObject<string>() is { } s
                        ? Parser.ConstantString(s)
                        : Unknown(node, key),
                    _ => Unknown(node, key)
                };
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
