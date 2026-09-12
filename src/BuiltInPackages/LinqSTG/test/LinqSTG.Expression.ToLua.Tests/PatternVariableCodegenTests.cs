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
        public void InfiniteNode_EmitsOuterScopeInfiniteAsScalar()
        {
            var lua = TranslateSingle(new NodeModel("Infinite", 0, 0, new JObject()));

            Assert.Equal("local __val = _infinite", lua);
        }

        [Fact]
        public void InfiniteNode_EditorValue_IsPreviewOnly_TranslationUnchanged()
        {
            // The on-node editor only feeds the preview: no matter its value,
            // translation still emits the outer-scope variable verbatim.
            var lua = TranslateSingle(new NodeModel("Infinite", 0, 0, new JObject { ["preview_value"] = 3 }));

            Assert.Equal("local __val = _infinite", lua);
            Assert.DoesNotContain("3", lua);
        }

        [Fact]
        public void SelfPositionNode_EmitsRedirectedSelfComponentsAsVector()
        {
            var lua = TranslateSingle(new NodeModel("SelfPosition", 0, 0, new JObject()));

            Assert.Equal("local __valx = __self.x\nlocal __valy = __self.y", lua);
        }

        [Fact]
        public void PlayerPositionNode_EmitsPlayerComponentsAsVector()
        {
            var lua = TranslateSingle(new NodeModel("PlayerPosition", 0, 0, new JObject()));

            Assert.Equal("local __valx = player.x\nlocal __valy = player.y", lua);
        }

        [Fact]
        public void Shoot_RedirectsSelfBeforeCreateAndAttachMovementShadowsIt()
        {
            // A real pattern connection is needed: only then does the Shoot
            // expansion contain the movement function that shadows self.
            var g = new TestGraph();
            var repeat = g.Add("RepeatPattern", new JObject { ["times"] = 1 });
            var shoot = g.Add("Shoot");
            g.Connect(repeat, "pattern", shoot, "pattern");
            var lua = g.BuildLua();

            var lines = lua.Split('\n').Select(l => l.TrimStart('\t')).ToArray();
            // The alias is emitted first, in the scope where self is still the
            // shooter; every later rebinding (the movement function's (self)
            // parameter, `local self = last`) only shadows `self`, not __self.
            var aliasIndex = Array.IndexOf(lines, "local __self = self");
            var shadowIndex = Array.IndexOf(lines, "__create_and_attach_movement(function(self)");
            Assert.True(aliasIndex >= 0, "Shoot must emit `local __self = self`");
            Assert.True(shadowIndex > aliasIndex, "the __self alias must precede the shadowing movement function");
        }

        [Fact]
        public void PositionNodes_CarryVectorShape_ForArithmeticDispatch()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var self = translator.Translate(new NodeModel("SelfPosition", 0, 0, new JObject()), new Dictionary<string, TypedLuaParser>());
            var player = translator.Translate(new NodeModel("PlayerPosition", 0, 0, new JObject()), new Dictionary<string, TypedLuaParser>());
            var add = translator.Translate(
                new NodeModel("Add", 0, 0, new JObject()),
                new Dictionary<string, TypedLuaParser>
                {
                    ["a"] = self,
                    ["b"] = player,
                });

            var text = string.Join("\n", add.LuaParser(Enumerable.Empty<LuaCodeLine>()).Select(l => l.Text));

            // Vector shape dispatches to the Vector2 variant of Add.
            Assert.Contains("local __valx = __self.x", text);
            Assert.Contains("local __valy = __self.y", text);
            Assert.Contains("local __valx = player.x", text);
            Assert.Contains("local __valx = __lhsx_1 + __rhsx_3", text);
            Assert.Contains("local __valy = __lhsy_2 + __rhsy_4", text);
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
                [
                    new VariableItemModel("_infinite", 10, true),
                    new VariableItemModel("self", 0, false, ValueY: 0),
                    new VariableItemModel("player", 0, false, ValueY: -180),
                    new VariableItemModel("speed", 2.5, false),
                ]);

            var json = JsonConvert.SerializeObject(model);
            Assert.Contains("\"variables\"", json);

            var roundTripped = JsonConvert.DeserializeObject<NetworkModel>(json)!;
            Assert.NotNull(roundTripped.Variables);
            Assert.Equal(4, roundTripped.Variables!.Length);
            Assert.Equal("_infinite", roundTripped.Variables[0].Name);
            Assert.True(roundTripped.Variables[0].IsInteger);
            Assert.Equal(10, roundTripped.Variables[0].Value);
            // Vector2 entries round-trip their Y component and keep it absent
            // for scalar entries.
            Assert.Null(roundTripped.Variables[0].ValueY);
            Assert.Null(roundTripped.Variables[3].ValueY);
            Assert.Equal(-180, roundTripped.Variables[2].ValueY);
            Assert.False(roundTripped.Variables[3].IsInteger);

            // Documents saved before the variable list existed still load: the
            // optional property deserializes to null.
            var legacy = JsonConvert.DeserializeObject<NetworkModel>(
                """{"nodes":[{"type":"Shoot","x":0,"y":0,"editors":{}}],"connections":[]}""")!;
            Assert.Null(legacy.Variables);
            Assert.Equal("Shoot", legacy.Nodes[0].NodeType);
        }
    }
}
