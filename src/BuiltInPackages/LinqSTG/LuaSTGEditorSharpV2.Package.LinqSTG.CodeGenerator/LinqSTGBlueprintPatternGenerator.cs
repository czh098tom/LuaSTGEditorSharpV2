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

        private static IEnumerable<CodeData> GenerateFromNetworkJson(NodeData node, string json, CodeGenerationContext context)
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

            var rootParser = NetworkCodeGenerator.ResolveRootParser(model);
            if (rootParser == null)
            {
                yield return new CodeData("--[[ no root node found in LinqSTG network ]]", node);
                yield break;
            }

            var content = BuildLua(rootParser, context);
            yield return new CodeData(content, node);
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
    }

    internal static class NetworkCodeGenerator
    {
        public static LuaParser? ResolveRootParser(NetworkModel model)
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

            var memo = new Dictionary<int, LuaParser>();
            var resolving = new HashSet<int>();

            LuaParser? ResolveNode(int idx)
            {
                if (memo.TryGetValue(idx, out var cached)) return cached;
                if (!resolving.Add(idx))
                {
                    return null;
                }
                var nodeModel = model.Nodes[idx];
                var inputs = ResolveInputs(idx);
                var parser = NodeTranslator.Translate(nodeModel, inputs);
                resolving.Remove(idx);
                memo[idx] = parser;
                return parser;
            }

            Dictionary<string, LuaParser> ResolveInputs(int idx)
            {
                var result = new Dictionary<string, LuaParser>(StringComparer.Ordinal);
                if (model.Connections == null) return result;
                foreach (var c in model.Connections)
                {
                    if (c is null) continue;
                    if (c.TargetNodeIndex != idx) continue;
                    if (ResolveNode(c.SourceNodeIndex) is { } src)
                    {
                        result[c.TargetPortName] = src;
                    }
                }
                return result;
            }

            int rootIdx = -1;
            for (int i = 0; i < model.Nodes.Length; i++)
            {
                if (model.Nodes[i].NodeType == "Shoot")
                {
                    rootIdx = i;
                    break;
                }
            }
            if (rootIdx < 0)
            {
                for (int i = 0; i < model.Nodes.Length; i++)
                {
                    if (!hasOutgoing.Contains(i))
                    {
                        rootIdx = i;
                        break;
                    }
                }
            }
            if (rootIdx < 0)
            {
                return null;
            }

            return ResolveNode(rootIdx);
        }
    }
}
