using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;

public sealed class CurvePointOutput(string? initialValue, string? previousValue, IEnumerable<CodeData> body)
{
    public string? InitialValue { get; } = initialValue;

    public string? PreviousValue { get; } = previousValue;

    public CodeFragment Body { get; } = new(
        body ?? throw new ArgumentNullException(nameof(body)));
}
