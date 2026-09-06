using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using LinqSTG.Expression.ToLua;
using LinqSTG.Expression.ToLua.Serialization;

using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.CodeGenerator
{
    public class LinqSTGBlueprintPatternGenerator(CodeGeneratorServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
        : CodeGeneratorServiceBase(nodeServiceProvider, serviceProvider)
    {
        public const string ShooterNodeTypeUID = "LinqSTGShooter";
        public const string ShooterNamePropertyKey = "name";
        public const string ShooterNameEditorKey = "shooter_name";

        protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            var json = node.GetProperty("source");
            if (string.IsNullOrWhiteSpace(json))
            {
                yield return new CodeData("--[[ empty LinqSTG blueprint ]]", node);
                yield break;
            }

            foreach (var cd in GenerateFromNetworkJson(node, json, context))
            {
                yield return cd;
            }
        }

        private IEnumerable<CodeData> GenerateFromNetworkJson(NodeData node, string json, CodeGenerationContext context)
        {
            NetworkModel? model;
            string? error;
            try
            {
                model = JsonConvert.DeserializeObject<NetworkModel>(json);
                error = null;
            }
            catch (Exception ex)
            {
                model = null;
                error = ex.Message;
            }
            if (error != null)
            {
                yield return new CodeData($"--[[ failed to deserialize LinqSTG network: {error} ]]", node);
                yield break;
            }
            if (model?.Nodes is null || model.Nodes.Length == 0)
            {
                yield return new CodeData("--[[ empty LinqSTG network ]]", node);
                yield break;
            }

            var shooterMap = BuildShooterMap(node);
            // One Parser per blueprint so its unique-id counter is scoped to this
            // single LinqSTG blueprint node rather than growing across the process.
            var parser = new Parser();
            var translator = new NodeTranslator(parser);
            var (rootParser, warnings) = NetworkCodeGenerator.ResolveRootParser(model, this, parser, translator, shooterMap, context);
            if (rootParser == null)
            {
                yield return new CodeData("--[[ no root node found in LinqSTG network ]]", node);
                yield break;
            }

            foreach (var w in warnings)
            {
                yield return new CodeData(w, node);
            }
            var content = BuildLua(rootParser, context);
            yield return new CodeData(content, node);
        }

        private static Dictionary<string, NodeData> BuildShooterMap(NodeData blueprintNode)
        {
            var map = new Dictionary<string, NodeData>(StringComparer.Ordinal);
            foreach (var child in blueprintNode.GetLogicalChildren())
            {
                if (child.TypeUID != ShooterNodeTypeUID) continue;
                var name = child.GetProperty(ShooterNamePropertyKey);
                if (string.IsNullOrEmpty(name)) continue;
                map[name] = child;
            }
            return map;
        }

        private static string BuildLua(LuaParser rootParser, CodeGenerationContext context)
        {
            var lines = rootParser(Enumerable.Empty<LuaCodeLine>());
            var finalBuilder = new StringBuilder();
            foreach (var line in lines)
            {
                var indention = context.GetIndented(line.Indent);
                var indented = context.ApplyIndented(indention, line.Text);
                finalBuilder.Append(indented);
            }
            return finalBuilder.ToString();
        }

        internal LuaParser WrapShootParser(
            NodeModel nodeModel,
            LuaParser originalParser,
            IReadOnlyDictionary<string, NodeData> shooterMap,
            CodeGenerationContext context,
            List<string> warnings)
        {
            var shooterName = GetShooterName(nodeModel);
            if (string.IsNullOrEmpty(shooterName))
            {
                warnings.Add($"--[[ ShootNode has no shooter name; skipped ]]");
                return _ => Enumerable.Empty<LuaCodeLine>();
            }
            if (!shooterMap.TryGetValue(shooterName, out var xNode))
            {
                warnings.Add($"--[[ shooter '{shooterName}' not found; skipped ]]");
                return _ => Enumerable.Empty<LuaCodeLine>();
            }
            var xCode = GenerateShooterChildrenCode(xNode, context).ToList();
            return _ => originalParser(xCode);
        }

        private static string GetShooterName(NodeModel nodeModel)
        {
            if (nodeModel.Editors.TryGetValue(ShooterNameEditorKey, out var token) && token != null)
            {
                return token.ToObject<string>() ?? string.Empty;
            }
            return string.Empty;
        }

        private IEnumerable<LuaCodeLine> GenerateShooterChildrenCode(NodeData shooterNode, CodeGenerationContext context)
        {
            var indentStr = context.Format("{0:IND}");
            if (string.IsNullOrEmpty(indentStr)) indentStr = "\t";
            int baseLevel = context.IndentionLevel;
            foreach (var cd in NodeServiceProvider.GenerateForChildren(shooterNode, context, 0))
            {
                var content = cd.Content;
                if (string.IsNullOrEmpty(content)) continue;
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.TrimEnd('\r');
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    int totalIndent = 0;
                    int pos = 0;
                    while (pos + indentStr.Length <= trimmed.Length
                           && trimmed.AsSpan(pos, indentStr.Length).SequenceEqual(indentStr))
                    {
                        totalIndent++;
                        pos += indentStr.Length;
                    }
                    int relativeIndent = Math.Max(0, totalIndent - baseLevel);
                    yield return new LuaCodeLine(trimmed.Substring(pos), relativeIndent);
                }
            }
        }
    }

    internal static class NetworkCodeGenerator
    {
        public static (LuaParser? rootParser, List<string> warnings) ResolveRootParser(
            NetworkModel model,
            LinqSTGBlueprintPatternGenerator generator,
            Parser parser,
            NodeTranslator translator,
            IReadOnlyDictionary<string, NodeData> shooterMap,
            CodeGenerationContext context)
        {
            var incoming = new Dictionary<(int, string), int>();
            var hasOutgoing = new HashSet<int>();
            if (model.Connections != null)
            {
                foreach (var c in model.Connections)
                {
                    if (c is null) continue;
                    incoming[(c.TargetNodeIndex, c.TargetPortName)] = c.SourceNodeIndex;
                    hasOutgoing.Add(c.SourceNodeIndex);
                }
            }

            var memo = new Dictionary<(int idx, string port), TypedLuaParser>();
            var resolving = new HashSet<int>();
            var warnings = new List<string>();

            bool IsEligibleShooter(int idx)
            {
                return incoming.ContainsKey((idx, "pattern"))
                    && incoming.ContainsKey((idx, "movement"))
                    && !hasOutgoing.Contains(idx);
            }

            // Resolves a node's output for a specific output port. Multi-output nodes
            // (e.g. Vector2Split) dispatch on portName; single-output nodes ignore it.
            TypedLuaParser? ResolveOutput(int idx, string portName)
            {
                var key = (idx, portName);
                if (memo.TryGetValue(key, out var cached)) return cached;
                if (!resolving.Add(idx)) return null;
                var nodeModel = model.Nodes[idx];
                var inputs = ResolveInputs(idx);
                var typed = translator.TranslateOutput(nodeModel, inputs, portName);
                // Shoot wrapping applies to the node's primary output regardless of port:
                // Shoot is single-output, so TranslateOutput delegates to Translate here.
                if (nodeModel.NodeType == "Shoot" && IsEligibleShooter(idx))
                {
                    var wrapped = generator.WrapShootParser(nodeModel, typed.LuaParser, shooterMap, context, warnings);
                    typed = typed with { LuaParser = wrapped };
                }
                resolving.Remove(idx);
                memo[key] = typed;
                return typed;
            }

            // Resolves a node by its primary (single) output, used for root nodes whose
            // output port name cannot be inferred from connections (root has no outgoing edge).
            TypedLuaParser? ResolveNode(int idx)
            {
                // Root/Shoot nodes are single-output; Translate ignores port, so any key
                // suffix works. Use the node type's convention via Translate directly to
                // avoid depending on a port name we don't have.
                if (memo.TryGetValue((idx, string.Empty), out var cached)) return cached;
                if (!resolving.Add(idx)) return null;
                var nodeModel = model.Nodes[idx];
                var inputs = ResolveInputs(idx);
                var typed = translator.Translate(nodeModel, inputs);
                if (nodeModel.NodeType == "Shoot" && IsEligibleShooter(idx))
                {
                    var wrapped = generator.WrapShootParser(nodeModel, typed.LuaParser, shooterMap, context, warnings);
                    typed = typed with { LuaParser = wrapped };
                }
                resolving.Remove(idx);
                memo[(idx, string.Empty)] = typed;
                return typed;
            }

            Dictionary<string, TypedLuaParser> ResolveInputs(int idx)
            {
                var result = new Dictionary<string, TypedLuaParser>(StringComparer.Ordinal);
                if (model.Connections == null) return result;
                foreach (var c in model.Connections)
                {
                    if (c is null) continue;
                    if (c.TargetNodeIndex != idx) continue;
                    if (ResolveOutput(c.SourceNodeIndex, c.SourcePortName) is { } src)
                    {
                        result[c.TargetPortName] = src;
                    }
                }
                return result;
            }

            // Every Shoot node is one emitter of the blueprint: they all get
            // translated (each with its own shooter-name-bound children code via
            // ResolveNode's WrapShootParser) and combined by ShootGroup into
            // do...end + shared __self prelude + one task.New per emitter.
            var shootIndices = new List<int>();
            for (int i = 0; i < model.Nodes.Length; i++)
            {
                if (model.Nodes[i].NodeType == "Shoot") shootIndices.Add(i);
            }
            if (shootIndices.Count > 0)
            {
                var shooters = shootIndices
                    .Select(i => ResolveNode(i)?.LuaParser)
                    .Where(p => p != null)
                    .Cast<LuaParser>()
                    .ToList();
                if (shooters.Count > 0)
                {
                    return (parser.ShootGroup(shooters), warnings);
                }
            }

            int rootIdx = -1;
            for (int i = 0; i < model.Nodes.Length; i++)
            {
                if (!hasOutgoing.Contains(i))
                {
                    rootIdx = i;
                    break;
                }
            }
            if (rootIdx < 0) return (null, warnings);

            return (ResolveNode(rootIdx)?.LuaParser, warnings);
        }
    }
}
