using LinqSTG.Expression.ToLua;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen tests for the RotateVector data node: the input Vector2's
    /// __valx/__valy are captured and rotated by a degree-based angle, with the
    /// angle coming from a wire or the editor constant, and the unconnected
    /// vector port degrading to the zero vector.
    /// </summary>
    public class RotateVectorCodegenTests
    {
        [Fact]
        public void RotateVector_WithWiredVector2_CapturesAndRotatesInDegrees()
        {
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 1f, ["y"] = 0f });
            var rotate = g.Add("RotateVector", new JObject { ["angle"] = 90f });
            g.Connect(vec, "vector2", rotate, "vector2");

            var lua = g.BuildLua(rotate);

            // The input vector's components are captured into unique locals ...
            Assert.Matches(@"local __vx_\d+, __vy_\d+", lua);
            Assert.Matches(@"__vx_\d+, __vy_\d+ = __valx, __valy", lua);
            // ... the unconnected angle falls back to the editor constant ...
            Assert.Contains("local __angle", lua);
            Assert.Contains("local __val = 90", lua);
            Assert.Contains("__angle = __val", lua);
            // ... and the rotation uses the runtime's degree-based cos/sin
            // (positive angle is counterclockwise; the preview negates Y on display).
            Assert.Matches(@"local __valx = __vx_\d+ \* cos\(__angle\) - __vy_\d+ \* sin\(__angle\)", lua);
            Assert.Matches(@"local __valy = __vx_\d+ \* sin\(__angle\) \+ __vy_\d+ \* cos\(__angle\)", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void RotateVector_WithUnconnectedVector2_DegeneratesToZeroVector()
        {
            var g = new TestGraph();
            var rotate = g.Add("RotateVector", new JObject { ["angle"] = 45f });

            var lua = g.BuildLua(rotate);

            Assert.Contains("local __valx, __valy = 0, 0", lua);
            Assert.Contains("cos(__angle)", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void RotateVector_WithWiredAngle_UsesConnectedParser()
        {
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 3f, ["y"] = 4f });
            var angle = g.Add("ConstantFloat", new JObject { ["value"] = 30f });
            var rotate = g.Add("RotateVector");
            g.Connect(vec, "vector2", rotate, "vector2");
            g.Connect(angle, "value", rotate, "angle");

            var lua = g.BuildLua(rotate);

            Assert.Contains("local __val = 30", lua);
            Assert.Contains("__angle = __val", lua);
            Assert.Contains("cos(__angle)", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }
    }
}
