using LinqSTG.Expression.ToLua;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen tests for the Vector2 variant of the Sample01MinMax node: the
    /// loop variable is normalized into [0,1] and component-wise remapped
    /// between two Vector2 endpoints, with unconnected bounds degrading to
    /// the zero vector.
    /// </summary>
    public class Sample01MinMaxVector2CodegenTests
    {
        [Fact]
        public void UnconnectedBounds_DegenerateToZeroVectors()
        {
            var g = new TestGraph();
            var node = g.Add("Sample01MinMaxVector2");

            var lua = g.BuildLua(node);

            // Two zero-vector locals, one per bound ...
            Assert.Contains("local __valx, __valy = 0, 0", lua);
            // ... captured into the two-component bound locals ...
            Assert.Contains("__lbx, __lby = __valx, __valy", lua);
            Assert.Contains("__ubx, __uby = __valx, __valy", lua);
            // ... sampled with the default repeater and HeadClosed interval ...
            Assert.Contains("local __max, __curr = __t, __i", lua);
            Assert.Contains("local __u = __max > 0 and (__curr / __max) or 0", lua);
            // ... and remapped component-wise.
            Assert.Contains("local __valx = __u * (__ubx - __lbx) + __lbx", lua);
            Assert.Contains("local __valy = __u * (__uby - __lby) + __lby", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void WiredBounds_CaptureConnectedVectors()
        {
            var g = new TestGraph();
            var lo = g.Add("Vector2", new JObject { ["x"] = -5f, ["y"] = -5f });
            var hi = g.Add("Vector2", new JObject { ["x"] = 5f, ["y"] = 5f });
            var node = g.Add("Sample01MinMaxVector2", new JObject { ["interval_type"] = 3 });
            g.Connect(lo, "vector2", node, "lower_bound");
            g.Connect(hi, "vector2", node, "upper_bound");

            var lua = g.BuildLua(node);

            Assert.Contains("__lbx, __lby = __valx, __valy", lua);
            Assert.Contains("__ubx, __uby = __valx, __valy", lua);
            Assert.DoesNotContain("local __valx, __valy = 0, 0", lua);
            // BothClosed (3) rescales the index by (total - 1).
            Assert.Contains("local __u = __max > 1 and (__curr / (__max - 1)) or 0", lua);
            Assert.Contains("local __valx = __u * (__ubx - __lbx) + __lbx", lua);
            Assert.Contains("local __valy = __u * (__uby - __lby) + __lby", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void WiredRepeater_UsesConnectedRepeaterText()
        {
            var g = new TestGraph();
            // The RepeaterKey translator ignores its string editors and emits
            // the ID/Total fallback literals, so the wired repeater expands to
            // "Total, ID" rather than the ambient __t, __i.
            var key = g.Add("RepeaterKey");
            var node = g.Add("Sample01MinMaxVector2");
            g.Connect(key, "repeater_key", node, "repeater");

            var lua = g.BuildLua(node);

            Assert.Contains("local __max, __curr = Total, ID", lua);
            Assert.DoesNotContain("__t, __i", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }
    }
}
