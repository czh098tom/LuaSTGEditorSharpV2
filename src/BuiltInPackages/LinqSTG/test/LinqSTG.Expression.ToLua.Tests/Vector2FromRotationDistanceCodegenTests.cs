using LinqSTG.Expression.ToLua;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen for the multi-output <c>Vector2FromRotationDistance</c> node: the
    /// "vector2" port keeps the <c>__valx</c>/<c>__valy</c> two-local form, while
    /// the "x"/"y" ports expose single components as scalars via the split
    /// primitives over the same angle/length body.
    /// </summary>
    public class Vector2FromRotationDistanceCodegenTests
    {
        private static string Render(TypedLuaParser typed)
            => string.Join("\n", typed.LuaParser(Enumerable.Empty<LuaCodeLine>()).Select(l => new string('\t', l.Indent) + l.Text));

        private static NodeModel FromEditors(float rotation, float distance)
            => new("Vector2FromRotationDistance", 0, 0, new JObject
            {
                ["rotation"] = rotation,
                ["distance"] = distance
            });

        [Fact]
        public void XPort_FromEditors_ProducesScalarComponent()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.TranslateOutput(
                FromEditors(90f, 2f),
                new Dictionary<string, TypedLuaParser>(),
                "x");

            Assert.Equal(PortShape.Scalar, typed.Shape);
            var lua = Render(typed);
            Assert.Contains("cos(__angle) * __length", lua);
            Assert.Matches(@"local __val = __vx_\d+", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void YPort_FromEditors_ProducesScalarComponent()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.TranslateOutput(
                FromEditors(90f, 2f),
                new Dictionary<string, TypedLuaParser>(),
                "y");

            Assert.Equal(PortShape.Scalar, typed.Shape);
            var lua = Render(typed);
            Assert.Contains("sin(__angle) * __length", lua);
            Assert.Matches(@"local __val = __vy_\d+", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void Vector2Port_KeepsTwoLocalForm()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.TranslateOutput(
                FromEditors(90f, 2f),
                new Dictionary<string, TypedLuaParser>(),
                "vector2");

            Assert.Equal(PortShape.Vector2, typed.Shape);
            var lua = Render(typed);
            Assert.Contains("local __valx = cos(__angle) * __length", lua);
            Assert.Contains("local __valy = sin(__angle) * __length", lua);
            Assert.DoesNotContain("__vx_", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void XPort_WithConnectedInputs_UsesResolvedValues()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var connectedRotation = translator.Translate(
                new NodeModel("ConstantFloat", 0, 0, new JObject { ["value"] = 30f }),
                new Dictionary<string, TypedLuaParser>());
            var typed = translator.TranslateOutput(
                new NodeModel("Vector2FromRotationDistance", 0, 0, new JObject { ["distance"] = 2f }),
                new Dictionary<string, TypedLuaParser> { ["rotation"] = connectedRotation },
                "x");

            var lua = Render(typed);
            // The wired rotation and the editor distance both reach the vector body.
            Assert.Contains("local __val = 30", lua);
            Assert.Contains("local __val = 2", lua);
            Assert.Matches(@"local __val = __vx_\d+", lua);
        }

        [Fact]
        public void XPort_FeedsArithmeticCombinators()
        {
            var g = new TestGraph();
            var polar = g.Add("Vector2FromRotationDistance", new JObject
            {
                ["rotation"] = 45f,
                ["distance"] = 3f
            });
            var constant = g.Add("ConstantFloat", new JObject { ["value"] = 1f });
            var add = g.Add("Add");
            g.Connect(polar, "x", add, "a");
            g.Connect(constant, "value", add, "b");

            var lua = g.BuildLua(forcedRoot: add);

            Assert.Contains("cos(__angle) * __length", lua);
            Assert.Matches(@"local __val = __lhs_\d+ \+ __rhs_\d+", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }
    }
}
