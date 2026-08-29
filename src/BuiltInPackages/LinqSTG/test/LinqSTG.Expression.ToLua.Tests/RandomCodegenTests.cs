using LinqSTG.Expression.ToLua;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen tests for the random generator nodes: RandomFloat emits
    /// ran:Float(start, end) sampling [start, end), RandomInt emits
    /// ran:Int(start, end) sampling [start, end] inclusive, and RandomSign
    /// emits ran:Sign() returning -1 or 1. Bounds come from wires or the
    /// node's editor constants.
    /// </summary>
    public class RandomCodegenTests
    {
        [Fact]
        public void RandomFloat_EditorBounds_EmitsRanFloat()
        {
            var g = new TestGraph();
            var node = g.Add("RandomFloat", new JObject { ["start"] = 1f, ["end"] = 10f });

            var lua = g.BuildLua(node);

            Assert.Contains("local __val = 1", lua);
            Assert.Contains("local __val = 10", lua);
            Assert.Matches(@"local __val = ran:Float\(__lhs_\d+, __rhs_\d+\)", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void RandomFloat_WiredStart_UsesConnectedParser()
        {
            var g = new TestGraph();
            var start = g.Add("ConstantFloat", new JObject { ["value"] = -5f });
            var node = g.Add("RandomFloat", new JObject { ["end"] = 5f });
            g.Connect(start, "value", node, "start");

            var lua = g.BuildLua(node);

            Assert.Contains("local __val = -5", lua);
            Assert.Contains("local __val = 5", lua);
            Assert.Matches(@"local __val = ran:Float\(__lhs_\d+, __rhs_\d+\)", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void RandomInt_EditorBounds_EmitsRanInt()
        {
            var g = new TestGraph();
            var node = g.Add("RandomInt", new JObject { ["start"] = 1, ["end"] = 6 });

            var lua = g.BuildLua(node);

            Assert.Contains("local __val = 1", lua);
            Assert.Contains("local __val = 6", lua);
            Assert.Matches(@"local __val = ran:Int\(__lhs_\d+, __rhs_\d+\)", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void RandomSign_NoInputs_EmitsRanSign()
        {
            var g = new TestGraph();
            var node = g.Add("RandomSign");

            var lua = g.BuildLua(node);

            Assert.Equal("local __val = ran:Sign()", lua);
        }

        [Fact]
        public void RandomSign_FeedsArithmeticCombinators()
        {
            var g = new TestGraph();
            var sign = g.Add("RandomSign");
            var scale = g.Add("ConstantFloat", new JObject { ["value"] = 3f });
            var multiply = g.Add("Multiply");
            g.Connect(sign, "value", multiply, "a");
            g.Connect(scale, "value", multiply, "b");

            var lua = g.BuildLua(multiply);

            Assert.Contains("local __val = ran:Sign()", lua);
            Assert.Contains("local __val = 3", lua);
            Assert.Matches(@"local __val = __lhs_\d+ \* __rhs_\d+", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }
    }
}
