using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public sealed class CurveGenerationException(
    string message,
    NodeData sourceNode,
    Exception? inner = null) : Exception(message, inner)
{
    public NodeData SourceNode { get; } = sourceNode;
}
