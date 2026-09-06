using LinqSTG.Expression.ToLua;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Codegen for multi-emitter blueprints: every Shoot node becomes one emitter.
    /// The whole output is wrapped in do...end, the shared `local __self = self`
    /// prelude is emitted once, and after it each emitter outputs its remaining
    /// part inside its own `task.New(self, function() ... end)` so the emitters
    /// run as independent coroutines.
    /// </summary>
    public class ShootGroupCodegenTests
    {
        private static string[] TrimmedLines(string lua)
            => lua.Split('\n').Select(l => l.TrimStart('\t')).ToArray();

        private static int Count(string[] lines, string text)
            => lines.Count(l => l == text);

        /// <summary>
        /// Adds a fully wired Shoot (pattern = RepeatPattern with the times input
        /// from a named pattern variable, movement = constant velocity). The
        /// variable name lets tests tell emitters apart in the generated Lua.
        /// </summary>
        private static void AddWiredShooter(TestGraph g, string timesVariable)
        {
            var times = g.Add("PatternVariableFloat", new JObject { ["name"] = timesVariable });
            var vec = g.Add("Vector2", new JObject { ["x"] = 1f, ["y"] = 2f });
            var vel = g.Add("UniformVelocityMovement");
            g.Connect(vec, "vector2", vel, "velocity");
            var repeat = g.Add("RepeatPattern");
            g.Connect(times, "value", repeat, "times");
            var shoot = g.Add("Shoot");
            g.Connect(repeat, "pattern", shoot, "pattern");
            g.Connect(vel, "movement", shoot, "movement");
        }

        [Fact]
        public void SingleShooter_IsWrappedInDoEndWithSharedSelfAndTask()
        {
            var g = new TestGraph();
            AddWiredShooter(g, "n");

            var lua = g.BuildLua();
            var lines = TrimmedLines(lua);

            Assert.Equal("do", lines.First());
            Assert.Equal("end", lines.Last());
            Assert.Equal("end)", lines[^2]);
            // Only __self is shared; it precedes the emitter's task.
            Assert.Equal(1, Count(lines, "local __self = self"));
            Assert.Equal(1, Count(lines, "task.New(self, function()"));
            // The aliases live inside the emitter's own task body, before its
            // movement-helper definition (the second `end)` closes the movement
            // function call).
            var taskIndex = Array.IndexOf(lines, "task.New(self, function()");
            var newTaskIndex = Array.IndexOf(lines, "local __new_task = function(fn) task.New(self, fn) end");
            var waitIndex = Array.IndexOf(lines, "local __wait = task.Wait");
            Assert.True(newTaskIndex > taskIndex, "__new_task must be declared inside the emitter's task");
            Assert.True(waitIndex > newTaskIndex);
            Assert.Equal(1, Count(lines, "local __create_and_attach_movement = function(fn)"));
            Assert.Equal(2, Count(lines, "end)"));
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void MultipleShooters_EmitSharedSelfOnce_AndAliasesPerShooter()
        {
            var g = new TestGraph();
            AddWiredShooter(g, "nA");
            AddWiredShooter(g, "nB");

            var lua = g.BuildLua();
            var lines = TrimmedLines(lua);

            Assert.Equal("do", lines.First());
            Assert.Equal("end", lines.Last());
            Assert.Equal("end)", lines[^2]);
            // __self stays shared; the overridable aliases and the movement helper
            // are per-emitter so one emitter's children code cannot corrupt another.
            Assert.Equal(1, Count(lines, "local __self = self"));
            Assert.Equal(2, Count(lines, "task.New(self, function()"));
            Assert.Equal(2, Count(lines, "local __new_task = function(fn) task.New(self, fn) end"));
            Assert.Equal(2, Count(lines, "local __wait = task.Wait"));
            Assert.Equal(2, Count(lines, "local __create_and_attach_movement = function(fn)"));
            // 2 movement-function closes + 2 task closes.
            Assert.Equal(4, Count(lines, "end)"));
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void MultipleShooters_KeepEachPatternAndAliasesInsideItsOwnTask()
        {
            var g = new TestGraph();
            AddWiredShooter(g, "nA");
            AddWiredShooter(g, "nB");

            var lines = TrimmedLines(g.BuildLua());

            var preludeIndex = Array.IndexOf(lines, "local __self = self");
            var firstTaskIndex = Array.IndexOf(lines, "task.New(self, function()");
            var secondTaskIndex = Array.LastIndexOf(lines, "task.New(self, function()");
            Assert.True(preludeIndex >= 0, "the shared prelude must be emitted");
            Assert.True(firstTaskIndex > preludeIndex, "every task must follow the prelude");
            Assert.True(secondTaskIndex > firstTaskIndex, "two emitters must each get a task");

            // Each emitter's aliases, pattern (its times variable) and movement-helper
            // definition live inside its own task body, never in the shared prelude.
            int nA = Array.IndexOf(lines, "local __val = nA");
            int defA = Array.IndexOf(lines, "local __create_and_attach_movement = function(fn)");
            int waitA = Array.IndexOf(lines, "local __wait = task.Wait");
            int nB = Array.IndexOf(lines, "local __val = nB");
            int defB = Array.LastIndexOf(lines, "local __create_and_attach_movement = function(fn)");
            int waitB = Array.LastIndexOf(lines, "local __wait = task.Wait");
            Assert.InRange(nA, firstTaskIndex, secondTaskIndex);
            Assert.InRange(defA, firstTaskIndex, secondTaskIndex);
            Assert.InRange(waitA, firstTaskIndex, secondTaskIndex);
            Assert.True(nB > secondTaskIndex, "the second emitter's pattern stays in the second task");
            Assert.True(defB > secondTaskIndex, "the second emitter's helper definition stays in the second task");
            Assert.True(waitB > secondTaskIndex, "the second emitter's aliases stay in the second task");
        }
    }
}
