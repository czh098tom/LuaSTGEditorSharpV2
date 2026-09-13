using LinqSTG.Expression.ToLua;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen for <c>ReversePattern</c>: the driving loop interleaves the
    /// reversed section's coroutines and records the timeline as wait markers.
    /// A pass where a coroutine only runs to completion (dies without yielding)
    /// advances no time and must not append a marker; gating on aliveness
    /// instead would inflate the timeline by one frame per Reverse layer, so
    /// nested Reverse would no longer round-trip the original timing.
    /// </summary>
    public class ReversePatternCodegenTests
    {
        private static string Translate(LuaParser parser)
        {
            var lines = parser(Enumerable.Empty<LuaCodeLine>());
            return string.Join("\n", lines.Select(l => new string('\t', l.Indent) + l.Text));
        }

        [Fact]
        public void DrivingLoop_CountsWaitMarkers_OnlyForPassesWhereACoroutineYielded()
        {
            var lua = Translate(new Parser().ReversePattern(new Parser().Empty()));

            Assert.Contains(
                "if coroutine.status(__rev_co[i]) == 'suspended' then __yielded = true end",
                lua);
            Assert.Contains("if __yielded then", lua);
            Assert.DoesNotContain("if __status then", lua);
        }
    }
}
