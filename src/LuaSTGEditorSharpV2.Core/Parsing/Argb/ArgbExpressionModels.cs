namespace LuaSTGEditorSharpV2.Core.Parsing.Argb;

public readonly record struct ArgbExpression(
    string Alpha,
    string Red,
    string Green,
    string Blue);

public readonly record struct ArgbPreviewChannel(
    byte Value,
    bool UsesExpressionFallback);

public readonly record struct ArgbPreview(
    ArgbPreviewChannel Alpha,
    ArgbPreviewChannel Red,
    ArgbPreviewChannel Green,
    ArgbPreviewChannel Blue)
{
    public bool UsesExpressionFallback =>
        Alpha.UsesExpressionFallback ||
        Red.UsesExpressionFallback ||
        Green.UsesExpressionFallback ||
        Blue.UsesExpressionFallback;
}

public sealed record ArgbExpressionForm(
    ArgbExpression? Components,
    ArgbPreview? Preview);
