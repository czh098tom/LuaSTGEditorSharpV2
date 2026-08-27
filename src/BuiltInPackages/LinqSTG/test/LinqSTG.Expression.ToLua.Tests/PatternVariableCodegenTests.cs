using LinqSTG.Expression.ToLua;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen for <c>PatternVariableFloat</c>/<c>PatternVariableInt</c> nodes
    /// (variable list entries dragged into the blueprint area): translation emits
    /// the entry's name verbatim as the scalar value, assuming it is a variable
    /// defined in the outer scope.
    /// </summary>
    public class PatternVariableCodegenTests
    {
        private static JObject NameEditor(string name) => new() { ["name"] = name };

        private static string TranslateSingle(NodeModel node)
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.Translate(node, new Dictionary<string, TypedLuaParser>());
            var lines = typed.LuaParser(Enumerable.Empty<LuaCodeLine>());
            return string.Join("\n", lines.Select(l => new string('\t', l.Indent) + l.Text));
        }

        [Fact]
        public void FloatVariant_EmitsOuterScopeVariableAsScalar()
        {
            var lua = TranslateSingle(new NodeModel("PatternVariableFloat", 0, 0, NameEditor("speed")));

            Assert.Equal("local __val = speed", lua);
        }

        [Fact]
        public void IntVariant_EmitsOuterScopeVariableAsScalar()
        {
            var lua = TranslateSingle(new NodeModel("PatternVariableInt", 0, 0, NameEditor("_infinite")));

            Assert.Equal("local __val = _infinite", lua);
        }

        [Fact]
        public void MissingNameEditor_FallsBackToUnknown()
        {
            var floatLua = TranslateSingle(new NodeModel("PatternVariableFloat", 0, 0, new JObject()));
            var intLua = TranslateSingle(new NodeModel("PatternVariableInt", 0, 0, new JObject()));

            Assert.Contains("UNKNOWN NODE", floatLua);
            Assert.Contains("UNKNOWN NODE", intLua);
        }

        [Fact]
        public void ScalarShape_FeedsArithmeticCombinators()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var speed = translator.Translate(new NodeModel("PatternVariableFloat", 0, 0, NameEditor("speed")), new Dictionary<string, TypedLuaParser>());
            var angle = translator.Translate(new NodeModel("PatternVariableInt", 0, 0, NameEditor("angle")), new Dictionary<string, TypedLuaParser>());
            var add = translator.Translate(
                new NodeModel("Add", 0, 0, new JObject()),
                new Dictionary<string, TypedLuaParser>
                {
                    ["a"] = speed,
                    ["b"] = angle,
                });

            var lines = add.LuaParser(Enumerable.Empty<LuaCodeLine>()).ToArray();
            var text = string.Join("\n", lines.Select(l => l.Text));

            Assert.Equal("local __val = speed", lines.First(l => l.Text.StartsWith("local __val = ", StringComparison.Ordinal) && l.Text.EndsWith("speed", StringComparison.Ordinal)).Text);
            Assert.Contains("local __val = angle", text);
            Assert.Contains("local __val = __lhs_1 + __rhs_2", text);
        }

        [Fact]
        public void NetworkModel_SerializesVariables_AndStaysBackCompatible()
        {
            var model = new NetworkModel(
                [new NodeModel("PatternVariableFloat", 0, 0, NameEditor("speed"))],
                [],
                [new VariableItemModel("_infinite", 10, true), new VariableItemModel("speed", 2.5, false)]);

            var json = JsonConvert.SerializeObject(model);
            Assert.Contains("\"variables\"", json);

            var roundTripped = JsonConvert.DeserializeObject<NetworkModel>(json)!;
            Assert.NotNull(roundTripped.Variables);
            Assert.Equal(2, roundTripped.Variables!.Length);
            Assert.Equal("_infinite", roundTripped.Variables[0].Name);
            Assert.True(roundTripped.Variables[0].IsInteger);
            Assert.Equal(10, roundTripped.Variables[0].Value);
            Assert.False(roundTripped.Variables[1].IsInteger);

            // Documents saved before the variable list existed still load: the
            // optional property deserializes to null.
            var legacy = JsonConvert.DeserializeObject<NetworkModel>(
                """{"nodes":[{"type":"Shoot","x":0,"y":0,"editors":{}}],"connections":[]}""")!;
            Assert.Null(legacy.Variables);
            Assert.Equal("Shoot", legacy.Nodes[0].NodeType);
        }
    }
}
