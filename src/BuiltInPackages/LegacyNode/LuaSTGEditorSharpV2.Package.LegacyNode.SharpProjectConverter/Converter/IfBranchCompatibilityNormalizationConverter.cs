using System.Linq;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

[JsonTypeShortName(typeof(ISharpNodeFormatConverter), "IfBranchCompatibilityNormalization")]
public sealed class IfBranchCompatibilityNormalizationConverter : ISharpNodeFormatConverter
{
    private enum BranchPriority
    {
        Then = -1,
        Neutral = 0,
        Else = 1
    }

    public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
    {
        // Disabled subtrees retain their physical order.
        for (NodeData? ancestor = source; ancestor != null; ancestor = ancestor.PhysicalParent)
        {
            if (!ancestor.IsActive)
            {
                return source;
            }
        }

        var before = source.GetLogicalChildren().ToArray();
        var after = before.OrderBy(GetPriority).ToArray();
        if (before.SequenceEqual(after))
        {
            return source;
        }

        // Reorder active slots only; disabled children keep their physical positions.
        var physicalChildren = source.PhysicalChildren.ToArray();
        var activeIndex = 0;
        for (var index = 0; index < physicalChildren.Length; index++)
        {
            if (physicalChildren[index].IsActive)
            {
                physicalChildren[index] = after[activeIndex++];
            }
        }

        for (var index = source.PhysicalChildren.Count - 1; index >= 0; index--)
        {
            source.Remove(index);
        }
        foreach (var child in physicalChildren)
        {
            source.Add(child);
        }

        context.ReportDiagnostic(source,
            $"[IfBranchCompatibilityNormalization] [{Describe(before)}] -> [{Describe(after)}]");
        return source;

        string Describe(IEnumerable<NodeData> nodes)
            => string.Join(", ", nodes.Select(node =>
                $"{SharpResourceMigrationAnalyzer.GetLegacyTypeName(node)}@{context.GetSourceLine(node)?.ToString() ?? "?"}"));
    }

    private static BranchPriority GetPriority(NodeData node)
        => node.TypeUID switch
        {
            "If.Then" => BranchPriority.Then,
            "If.Else" => BranchPriority.Else,
            _ => SharpResourceMigrationAnalyzer.GetLegacyTypeName(node) switch
            {
                "IfThen" => BranchPriority.Then,
                "IfElse" => BranchPriority.Else,
                _ => BranchPriority.Neutral
            }
        };
}
