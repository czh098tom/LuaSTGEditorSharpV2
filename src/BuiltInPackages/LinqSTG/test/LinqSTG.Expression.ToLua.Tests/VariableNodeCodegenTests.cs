using LinqSTG.Expression.ToLua;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen for the multi-output <c>Variable</c> node: the "key" output passes
    /// the resolved variable name through as bare Lua text, while the "value"
    /// output binds it via TakeVariableFromContext like the standalone node.
    /// </summary>
    public class VariableNodeCodegenTests
    {
        private static JObject KeyEditor(string name) => new() { ["key"] = name };

        private static string Render(TypedLuaParser typed)
            => string.Join("\n", typed.LuaParser(Enumerable.Empty<LuaCodeLine>()).Select(l => new string('\t', l.Indent) + l.Text));

        [Fact]
        public void ValuePort_FromEditor_EmitsOuterScopeVariableAsScalar()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.TranslateOutput(
                new NodeModel("Variable", 0, 0, KeyEditor("speed")),
                new Dictionary<string, TypedLuaParser>(),
                "value");

            Assert.Equal(PortShape.Scalar, typed.Shape);
            Assert.Equal("local __val = speed", Render(typed));
        }

        [Fact]
        public void KeyPort_FromEditor_PassesNameThroughAsBareText()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.TranslateOutput(
                new NodeModel("Variable", 0, 0, KeyEditor("speed")),
                new Dictionary<string, TypedLuaParser>(),
                "key");

            Assert.Equal("speed", Render(typed));
        }

        [Fact]
        public void ValuePort_WithConnectedKey_UsesResolvedKey()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var connectedKey = translator.Translate(
                new NodeModel("ConstantString", 0, 0, new JObject { ["value"] = "angle" }),
                new Dictionary<string, TypedLuaParser>());
            var typed = translator.TranslateOutput(
                new NodeModel("Variable", 0, 0, new JObject()),
                new Dictionary<string, TypedLuaParser> { ["key"] = connectedKey },
                "value");

            Assert.Equal("local __val = angle", Render(typed));
        }

        [Fact]
        public void UnknownPort_FallsBackToUnknown()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.TranslateOutput(
                new NodeModel("Variable", 0, 0, KeyEditor("speed")),
                new Dictionary<string, TypedLuaParser>(),
                "other");

            Assert.Contains("UNKNOWN NODE", Render(typed));
        }

        [Fact]
        public void ValuePort_FeedsArithmeticCombinators()
        {
            var g = new TestGraph();
            var variable = g.Add("Variable", KeyEditor("speed"));
            var constant = g.Add("ConstantFloat", new JObject { ["value"] = 1f });
            var add = g.Add("Add");
            g.Connect(variable, "value", add, "a");
            g.Connect(constant, "value", add, "b");

            var lua = g.BuildLua(forcedRoot: add);

            Assert.Contains("local __val = speed", lua);
            Assert.Contains("local __val = __lhs_1 + __rhs_2", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void KeyPort_FeedsTakeVariableFromContext()
        {
            var g = new TestGraph();
            var variable = g.Add("Variable", KeyEditor("speed"));
            var take = g.Add("TakeVariableFromContext");
            g.Connect(variable, "key", take, "key");

            var lua = g.BuildLua(forcedRoot: take);

            Assert.Contains("local __val = speed", lua);
        }
    }
}
