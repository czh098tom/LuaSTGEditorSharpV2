using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;

internal sealed class ComponentPattern(
    ParserOptions options)
    : IPattern<LuaExpressionToken, LuaExpressionToken[][]>
{
    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[]> Component =
            Parser<LuaExpressionToken>.Token(token => !IsComma(token))
                .Many()
                .Capture()
                .Map(LuaExpressionTokenSequence.Trim);

    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[][]> Matcher =
            Parser.Map(
                (first,
remaining) =>
                    remaining
                        .Prepend(first)
                        .Where(tokens => tokens.Length > 0)
                        .ToArray(),
                Component,
                LuaExpressionTokenParsers
                    .Atom(
                        LuaExpressionAtomKind.Punctuation,
                        ",")
                    .Then(Component)
                    .Many())
            .Before(Parser<LuaExpressionToken>.End);

    public LuaExpressionToken[][]? Match(
        IEnumerable<LuaExpressionToken> input)
        => Matcher.ParseOrNull(UnwrapWholeParentheses(input));

    public IEnumerable<LuaExpressionToken> Reconstruct(
        LuaExpressionToken[][] form)
        => LuaExpressionTokenSequence.Intercalate(
            form,
            Separator());

    private IEnumerable<LuaExpressionToken> Separator()
        => options.SpaceAfterComma
            ?
            [
                new LuaExpressionAtom(
                    ",",
                    LuaExpressionAtomKind.Punctuation),
                new LuaExpressionAtom(
                    " ",
                    LuaExpressionAtomKind.Whitespace),
            ]
            :
            [
                new LuaExpressionAtom(
                    ",",
                    LuaExpressionAtomKind.Punctuation),
            ];

    private static LuaExpressionToken[] UnwrapWholeParentheses(
        IEnumerable<LuaExpressionToken> input)
    {
        var tokens = LuaExpressionTokenSequence.Trim(input);
        return tokens is
        [
            LuaExpressionGroup
            {
                Kind: LuaExpressionGroupKind.Parentheses,
            } group,
        ]
            ? UnwrapWholeParentheses(group.Children)
            : tokens;
    }

    private static bool IsComma(LuaExpressionToken token)
        => token is LuaExpressionAtom
        {
            Kind: LuaExpressionAtomKind.Punctuation,
            Text: ",",
        };
}
