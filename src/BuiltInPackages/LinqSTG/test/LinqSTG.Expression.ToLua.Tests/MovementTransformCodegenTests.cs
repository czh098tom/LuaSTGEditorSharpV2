using LinqSTG.Expression.ToLua;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    /// <summary>
    /// Miniature replica of the production <c>NetworkCodeGenerator</c> resolution flow
    /// (connection-driven input resolution + root selection, without Shoot wrapping),
    /// so tests exercise the same port-name contracts as real blueprint translation.
    /// </summary>
    internal sealed class TestGraph
    {
        private readonly List<NodeModel> _nodes = [];
        private readonly List<ConnectionModel> _connections = [];

        public int Add(string type, JObject? editors = null)
        {
            _nodes.Add(new NodeModel(type, 0, 0, editors ?? []));
            return _nodes.Count - 1;
        }

        public void Connect(int source, string sourcePort, int target, string targetPort)
        {
            _connections.Add(new ConnectionModel(source, sourcePort, target, targetPort));
        }

        /// <summary>Builds the Lua for the root node (no outgoing edges), or a forced root.</summary>
        public string BuildLua(int? forcedRoot = null)
        {
            var model = new NetworkModel([.. _nodes], [.. _connections]);
            var parser = new Parser();
            var translator = new NodeTranslator(parser);

            var incoming = new Dictionary<(int, string), int>();
            var hasOutgoing = new HashSet<int>();
            foreach (var c in model.Connections)
            {
                incoming[(c.TargetNodeIndex, c.TargetPortName)] = c.SourceNodeIndex;
                hasOutgoing.Add(c.SourceNodeIndex);
            }

            var memo = new Dictionary<(int idx, string port), TypedLuaParser>();
            var resolving = new HashSet<int>();

            TypedLuaParser? ResolveOutput(int idx, string portName)
            {
                var key = (idx, portName);
                if (memo.TryGetValue(key, out var cached)) return cached;
                if (!resolving.Add(idx)) return null;
                var inputs = ResolveInputs(idx);
                var typed = translator.TranslateOutput(model.Nodes[idx], inputs, portName);
                resolving.Remove(idx);
                memo[key] = typed;
                return typed;
            }

            Dictionary<string, TypedLuaParser> ResolveInputs(int idx)
            {
                var result = new Dictionary<string, TypedLuaParser>(StringComparer.Ordinal);
                foreach (var c in model.Connections)
                {
                    if (c.TargetNodeIndex != idx) continue;
                    if (ResolveOutput(c.SourceNodeIndex, c.SourcePortName) is { } src)
                    {
                        result[c.TargetPortName] = src;
                    }
                }
                return result;
            }

            string Join(IEnumerable<LuaCodeLine> lines)
                => string.Join("\n", lines.Select(l => new string('\t', l.Indent) + l.Text));

            // Mirrors production root selection: every Shoot node becomes one
            // emitter combined by ShootGroup (without the generator's shooter
            // children wrapping, which TestGraph does not replicate).
            if (forcedRoot is null)
            {
                var shooters = Enumerable.Range(0, model.Nodes.Length)
                    .Where(i => model.Nodes[i].NodeType == "Shoot")
                    .Select(i => translator.Translate(model.Nodes[i], ResolveInputs(i)).LuaParser)
                    .ToList();
                if (shooters.Count > 0)
                {
                    return Join(parser.ShootGroup(shooters)(Enumerable.Empty<LuaCodeLine>()));
                }
            }

            var rootIdx = forcedRoot
                ?? Enumerable.Range(0, model.Nodes.Length).FirstOrDefault(i => !hasOutgoing.Contains(i), -1);
            Assert.True(rootIdx >= 0, "graph has no root (all nodes have outgoing connections)");
            var rootInputs = ResolveInputs(rootIdx);
            var root = translator.Translate(model.Nodes[rootIdx], rootInputs);
            return Join(root.LuaParser(Enumerable.Empty<LuaCodeLine>()));
        }
    }

    /// <summary>
    /// Codegen tests for the unified Movement-Predict-FromPointMovement custom
    /// transform path: FromPointMovement expands its point subgraph per sample
    /// time, MovementTransformInputTime reads the ambient __t, and MovementPredict
    /// rebinds __t to sample a wired movement at any t'.
    /// </summary>
    public class MovementTransformCodegenTests
    {
        private static string LastLineStartingWith(string lua, string prefix)
        {
            var line = lua.Split('\n').FirstOrDefault(l => l.TrimStart('\t').StartsWith(prefix, StringComparison.Ordinal));
            Assert.NotNull(line);
            return line.TrimStart('\t');
        }

        [Fact]
        public void MovementTransformInputTime_ReadsAmbientTime()
        {
            var g = new TestGraph();
            g.Add("MovementTransformInputTime");
            Assert.Equal("local __val = __t", g.BuildLua());
        }

        [Fact]
        public void MovementPredict_WithUnconnectedMovement_DegeneratesToZeroPoint()
        {
            var g = new TestGraph();
            g.Add("MovementPredict");
            var lua = g.BuildLua();

            Assert.Equal("local __valx, __valy = 0, 0", LastLineStartingWith(lua, "local __valx"));
            Assert.DoesNotContain("__tpm", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void MovementPredict_WithConnectedMovement_RebindsTimeInShadowedScope()
        {
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 1f, ["y"] = 2f });
            var movement = g.Add("UniformVelocityMovement");
            g.Connect(vec, "vector2", movement, "velocity");
            var time = g.Add("ConstantInt", new JObject { ["value"] = 5 });
            var predict = g.Add("MovementPredict");
            g.Connect(movement, "movement", predict, "movement");
            g.Connect(time, "value", predict, "time");

            var lua = g.BuildLua(predict);

            // Connected form: shadow __x/__y/__t, sample the wired movement at t'.
            Assert.Contains("local __x, __y", lua);
            Assert.Matches(@"local __t = __tp_t_\d+", LastLineStartingWith(lua, "local __t = __tp_t_"));
            Assert.DoesNotContain("__tpm", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void FromPointMovement_WrapsPointSubgraphIntoConstantMovement()
        {
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 3f, ["y"] = 4f });
            var fromPoint = g.Add("FromPointMovement");
            g.Connect(vec, "vector2", fromPoint, "position");

            var lua = g.BuildLua(fromPoint);

            Assert.Contains("__x = __valx", lua);
            Assert.Contains("__y = __valy", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void StationaryMovement_NodeType_IsNoLongerTranslated()
        {
            var g = new TestGraph();
            g.Add("StationaryMovement");
            Assert.Contains("UNKNOWN NODE: StationaryMovement", g.BuildLua());
        }

        [Fact]
        public void MovementMap_AndTransformFromMovement_NodeTypes_AreNoLongerTranslated()
        {
            var map = new TestGraph();
            map.Add("MovementMap");
            Assert.Contains("UNKNOWN NODE: MovementMap", map.BuildLua());

            var fromMovement = new TestGraph();
            fromMovement.Add("MovementTransformFromMovement");
            Assert.Contains("UNKNOWN NODE: MovementTransformFromMovement", fromMovement.BuildLua());
        }

        [Fact]
        public void UnifiedPath_Identity_ComposesPredictInputTimeAndFromPointMovement()
        {
            // FromPointMovement(Predict(UniformVelocity(1,2), InputTime))
            // i.e. the canonical Movement-Predict-FromPointMovement custom transform (identity).
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 1f, ["y"] = 2f });
            var source = g.Add("UniformVelocityMovement");
            g.Connect(vec, "vector2", source, "velocity");
            var time = g.Add("MovementTransformInputTime");
            var predict = g.Add("MovementPredict");
            g.Connect(source, "movement", predict, "movement");
            g.Connect(time, "time", predict, "time");
            var fromPoint = g.Add("FromPointMovement");
            g.Connect(predict, "point", fromPoint, "position");

            var lua = g.BuildLua(fromPoint);

            // InputTime reads the ambient __t inside FromPointMovement's point subgraph ...
            Assert.Contains("local __val = __t", lua);
            // ... Predict rebinds __t inside a shadowed scope to sample the wired movement ...
            Assert.Contains("local __x, __y", lua);
            Assert.Matches(@"local __t = __tp_t_\d+", LastLineStartingWith(lua, "local __t = __tp_t_"));
            // ... and FromPointMovement wraps the sampled point back into __x/__y.
            Assert.Contains("__x = __valx", lua);
            Assert.Contains("__y = __valy", lua);
            Assert.DoesNotContain("__tpm", lua);
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void MovementScaleTime_RebindsAmbientTime()
        {
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 1f, ["y"] = 2f });
            var movement = g.Add("UniformVelocityMovement");
            g.Connect(vec, "vector2", movement, "velocity");
            var scaleTime = g.Add("MovementScaleTime", new JObject { ["factor"] = 2f });
            g.Connect(movement, "movement", scaleTime, "movement");

            var lua = g.BuildLua(scaleTime);

            Assert.Matches(@"local __t = __st_a_\d+ \* __t", LastLineStartingWith(lua, "local __t = __st_a_"));
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }

        [Fact]
        public void MovementShiftTime_RebindsAmbientTime()
        {
            var g = new TestGraph();
            var vec = g.Add("Vector2", new JObject { ["x"] = 1f, ["y"] = 2f });
            var movement = g.Add("UniformVelocityMovement");
            g.Connect(vec, "vector2", movement, "velocity");
            var shiftTime = g.Add("MovementShiftTime", new JObject { ["delta"] = 3 });
            g.Connect(movement, "movement", shiftTime, "movement");

            var lua = g.BuildLua(shiftTime);

            Assert.Matches(@"local __t = __t \+ __st_d_\d+", LastLineStartingWith(lua, "local __t = __t + __st_d_"));
            Assert.DoesNotContain("UNKNOWN NODE", lua);
        }
    }
}
