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
                    InputOrConstant(node, inputs, "upper_bound")),

                "Sample01" => Parser.Sample01(
                    InputOrDefaultRepeater(inputs, "repeater")),

                "MinMax" => Parser.MinMax(
                    InputOrUnknown(node, inputs, "input_value"),
                    InputOrConstant(node, inputs, "lower_bound"),
                    InputOrConstant(node, inputs, "upper_bound")),

                "TakeRepeaterFromContext" => Parser.TakeRepeaterFromContext(
                    InputOrUnknown(node, inputs, "repeater_key")),

                "Vector2FromRotationDistance" => Parser.VectorFromAngleLength(
                    InputOrConstant(node, inputs, "rotation"),
                    InputOrConstant(node, inputs, "distance")),

                "ConstantFloat" => ConstantFromEditor(node, "value"),

                "ConstantInt" => ConstantFromEditor(node, "value"),

                "ConstantString" => ConstantFromEditor(node, "value"),

                "Add" => Parser.IntrinsicAdd(
                    InputOrUnknown(node, inputs, "a"),
                    InputOrUnknown(node, inputs, "b")),

                "UniformVelocityMovement" => Parser.UniformVelocityMovement(
                    InputOrUnknown(node, inputs, "velocity")),

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
    }
}
