using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua;

internal static class LuaExpressionTokenParsers
{
    public static readonly Parser<LuaExpressionToken, LuaExpressionToken>
        Identifier = Atom(LuaExpressionAtomKind.Identifier);

    public static readonly Parser<LuaExpressionToken, LuaExpressionToken>
        Whitespace = Atom(LuaExpressionAtomKind.Whitespace);

    public static readonly Parser<LuaExpressionToken, Unit>
        SkipWhitespace = Whitespace.SkipMany();

    public static Parser<LuaExpressionToken, LuaExpressionToken> Atom(
        LuaExpressionAtomKind kind,
        string? text = null)
        => Parser<LuaExpressionToken>.Token(token =>
            token is LuaExpressionAtom atom
            && atom.Kind == kind
            && (text is null || atom.Text == text));

    public static Parser<LuaExpressionToken, LuaExpressionGroup> Group(
        LuaExpressionGroupKind kind)
        => Parser<LuaExpressionToken>.Token(token =>
                token is LuaExpressionGroup group
                && group.Kind == kind)
            .Map(token => (LuaExpressionGroup)token);

    public static Parser<LuaExpressionToken, LuaExpressionToken[]> Capture<T>(
        this Parser<LuaExpressionToken, T> parser)
        => parser.Slice((tokens, _) => tokens.ToArray());
}
