using LinqSTG.Expression.ToLua;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen for the aggregated vector2 variable nodes:
    /// <c>Vector2Variable</c> bundles two <c>Variable</c> nodes (key passthrough +
    /// TakeVariableFromContext value outputs) plus a vector2 output that reads both
    /// outer-scope variables into __valx/__valy; <c>Vector2Assignment</c> writes a
    /// vector input's components to the two editor-backed variable names.
    /// </summary>
    public class Vector2VariableNodeCodegenTests
    {
        private static JObject Keys(string x, string y) => new() { ["key_x"] = x, ["key_y"] = y };

        private static string Render(TypedLuaParser typed)
            => string.Join("\n", typed.LuaParser(Enumerable.Empty<LuaCodeLine>()).Select(l => new string('\t', l.Indent) + l.Text));

        [Fact]
        public void Vector2Variable_Vector2Port_FromEditors_ReadsBothOuterVariables()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.TranslateOutput(
                new NodeModel("Vector2Variable", 0, 0, Keys("vx", "vy")),
                new Dictionary<string, TypedLuaParser>(),
                "vector2");

            Assert.Equal(PortShape.Vector2, typed.Shape);
            Assert.Equal("local __valx = vx\nlocal __valy = vy", Render(typed));
        }

        [Fact]
        public void Vector2Variable_ComponentPorts_BindKeysViaTakeVariable()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var node = new NodeModel("Vector2Variable", 0, 0, Keys("vx", "vy"));

            var x = translator.TranslateOutput(node, new Dictionary<string, TypedLuaParser>(), "x");
            var y = translator.TranslateOutput(node, new Dictionary<string, TypedLuaParser>(), "y");

            Assert.Equal(PortShape.Scalar, x.Shape);
            Assert.Equal("local __val = vx", Render(x));
            Assert.Equal("local __val = vy", Render(y));
        }

        [Fact]
        public void Vector2Variable_KeyPorts_PassNamesThroughAsBareText()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var node = new NodeModel("Vector2Variable", 0, 0, Keys("vx", "vy"));

            var kx = translator.TranslateOutput(node, new Dictionary<string, TypedLuaParser>(), "key_x");
            var ky = translator.TranslateOutput(node, new Dictionary<string, TypedLuaParser>(), "key_y");

            Assert.Equal("vx", Render(kx));
            Assert.Equal("vy", Render(ky));
        }

        [Fact]
        public void Vector2Variable_Vector2Port_WithConnectedKeys_UsesResolvedKeys()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var connectedX = translator.Translate(
                new NodeModel("ConstantString", 0, 0, new JObject { ["value"] = "px" }),
                new Dictionary<string, TypedLuaParser>());
            var typed = translator.TranslateOutput(
                new NodeModel("Vector2Variable", 0, 0, new JObject { ["key_y"] = "py" }),
                new Dictionary<string, TypedLuaParser> { ["key_x"] = connectedX },
                "vector2");

            Assert.Equal("local __valx = px\nlocal __valy = py", Render(typed));
        }

        [Fact]
        public void Vector2Variable_Vector2Port_FeedsVectorConsumers()
        {
            var g = new TestGraph();
            var variable = g.Add("Vector2Variable", Keys("vx", "vy"));
            var rotate = g.Add("RotateVector", new JObject { ["angle"] = 0f });
            g.Connect(variable, "vector2", rotate, "vector2");

            var lua = g.BuildLua(forcedRoot: rotate);

            // The consumer reads the __valx/__valy form emitted by the variable's
            // vector2 output.
            Assert.Contains("local __valx = vx", lua);
            Assert.Contains("local __valy = vy", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void Vector2Assignment_FromEditors_WritesBothOuterVariables()
        {
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var typed = translator.Translate(
                new NodeModel("Vector2Assignment", 0, 0, Keys("px", "py")),
                new Dictionary<string, TypedLuaParser>());

            // Unconnected value degrades to the zero vector, then both components
            // are written to their named outer-scope variables.
            var lua = Render(typed);
            Assert.Contains("local __valx, __valy = 0, 0", lua);
            Assert.Contains("local px = ", lua);
            Assert.Contains("local py = ", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void Vector2Assignment_WithVectorValue_ComponentsGoToNamedVariables()
        {
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 3f, ["y"] = 4f });
            var assign = g.Add("Vector2Assignment", Keys("px", "py"));
            g.Connect(vec, "vector2", assign, "value");

            var lua = g.BuildLua(forcedRoot: assign);

            // The vector input's editor constants flow through, then both
            // components are written to their named outer-scope variables.
            Assert.Contains("local __val = 3", lua);
            Assert.Contains("local __val = 4", lua);
            Assert.Matches(@"local px = __vx_\d+", lua);
            Assert.Matches(@"local py = __vy_\d+", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void Vector2Assignment_ChainsPreviousTransformation()
        {
            var g = new TestGraph();
            var floatValue = g.Add("ConstantFloat", new JObject { ["value"] = 7f });
            var stringKey = g.Add("ConstantString", new JObject { ["value"] = "speed" });
            var prev = g.Add("Assign");
            g.Connect(floatValue, "value", prev, "value");
            g.Connect(stringKey, "value", prev, "key");
            var vec = g.Add("Vector2", new JObject { ["x"] = 1f, ["y"] = 2f });
            var assign = g.Add("Vector2Assignment", Keys("px", "py"));
            g.Connect(vec, "vector2", assign, "value");
            g.Connect(prev, "transformation", assign, "transformation");

            var lua = g.BuildLua(forcedRoot: assign);

            // The chained scalar Assign runs first, then the vector2 assignment
            // extends the transformation with both named variables.
            Assert.Contains("local speed = ", lua);
            Assert.Contains("local px = ", lua);
            Assert.Contains("local py = ", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }
    }
}
