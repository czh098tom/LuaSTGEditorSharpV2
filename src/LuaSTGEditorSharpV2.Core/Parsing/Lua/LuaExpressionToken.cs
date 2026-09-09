using System.Collections.ObjectModel;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua;

public abstract class LuaExpressionToken
{
    protected LuaExpressionToken(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

public enum LuaExpressionAtomKind
{
    Identifier,
    Keyword,
    Number,
    String,
    Operator,
    Punctuation,
    Whitespace,
}

public sealed class LuaExpressionAtom : LuaExpressionToken
{
    public LuaExpressionAtom(string text, LuaExpressionAtomKind kind)
        : base(text)
    {
        Kind = kind;
    }

    internal LuaExpressionAtom(
        string text,
        LuaExpressionAtomKind kind,
        object value)
        : base(text)
    {
        Kind = kind;
        Value = value;
    }

    public LuaExpressionAtomKind Kind { get; }

    public object? Value { get; }
}

public enum LuaExpressionGroupKind
{
    Parentheses,
    Brackets,
    Braces,
}

public sealed class LuaExpressionGroup : LuaExpressionToken
{
    public LuaExpressionGroup(
        LuaExpressionGroupKind kind,
        string openingText,
        string closingText,
        IReadOnlyList<LuaExpressionToken> children)
        : this(
            kind,
            openingText,
            closingText,
            Array.AsReadOnly(children.ToArray()))
    {
    }

    private LuaExpressionGroup(
        LuaExpressionGroupKind kind,
        string openingText,
        string closingText,
        ReadOnlyCollection<LuaExpressionToken> children)
        : base(openingText + string.Concat(children.Select(token => token.Text)) + closingText)
    {
        Kind = kind;
        OpeningText = openingText;
        ClosingText = closingText;
        Children = children;
    }

    public LuaExpressionGroupKind Kind { get; }

    public string OpeningText { get; }

    public string ClosingText { get; }

    public IReadOnlyList<LuaExpressionToken> Children { get; }
}
