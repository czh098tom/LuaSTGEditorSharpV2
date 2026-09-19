using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public sealed class AdvancedRepeatGenerationException(
    string message, NodeData sourceNode, System.Exception? inner = null) : System.Exception(message, inner)
{
    public NodeData SourceNode { get; } = sourceNode;
}
