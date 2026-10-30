using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public sealed class CurveOutput(IEnumerable<CodeData> declaration, IEnumerable<CodeData> task)
{
    public IEnumerable<CodeData> Declaration { get; } =
        declaration ?? throw new ArgumentNullException(nameof(declaration));

    public IEnumerable<CodeData> Task { get; } =
        task ?? throw new ArgumentNullException(nameof(task));
}
