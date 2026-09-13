namespace LuaSTGEditorSharpV2.Core.Parsing.Argb;

public readonly record struct ArgbExpression(
    string Alpha,
    string Red,
    string Green,
    string Blue);

public readonly record struct ArgbPreview(
    byte Alpha,
    byte Red,
    byte Green,
    byte Blue);
