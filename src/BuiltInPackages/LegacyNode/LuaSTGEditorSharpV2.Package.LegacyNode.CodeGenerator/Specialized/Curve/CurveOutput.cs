using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public sealed class CurveOutput(IEnumerable<CodeData> declaration, IEnumerable<CodeData> task)
{
    public CodeFragment Declaration { get; } = new(
        declaration ?? throw new ArgumentNullException(nameof(declaration)));

    public CodeFragment Task { get; } = new(
        task ?? throw new ArgumentNullException(nameof(task)));
}
