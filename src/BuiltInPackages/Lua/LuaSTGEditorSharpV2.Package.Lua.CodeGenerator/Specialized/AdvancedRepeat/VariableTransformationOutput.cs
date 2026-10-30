using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Specialized.AdvancedRepeat;

public sealed class VariableTransformationOutput(IEnumerable<CodeData> initialization, IEnumerable<CodeData> update)
{
    public IEnumerable<CodeData> Initialization { get; } =
        initialization ?? throw new ArgumentNullException(nameof(initialization));
    public IEnumerable<CodeData> Update { get; } =
        update ?? throw new ArgumentNullException(nameof(update));
}
