using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public sealed class VariableTransformationOutput(IEnumerable<CodeData> initialization, IEnumerable<CodeData> update)
{
    public CodeFragment Initialization { get; } = new(initialization);
    public CodeFragment Update { get; } = new(update);
}
