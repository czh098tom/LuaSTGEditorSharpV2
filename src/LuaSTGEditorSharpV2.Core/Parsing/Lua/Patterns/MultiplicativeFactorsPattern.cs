using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;

internal sealed class MultiplicativeFactorsPattern(
    ParserOptions options)
    : IPattern<LuaExpressionToken, LuaExpressionToken[][]>
{
    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[]> Factor =
            Parser<LuaExpressionToken>.Token(
                    token => !IsMultiplication(token))
                .Many()
                .Capture()
                .Map(LuaExpressionTokenSequence.Trim);

    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[][]> Matcher =
            Parser.Map(
                (first,
remaining) =>
                {
                    var rest = remaining.ToArray();
                    return first.Length == 0 && rest.Length == 0
                        ? []
                        : rest.Prepend(first).ToArray();
                },
                Factor,
                LuaExpressionTokenParsers
                    .Atom(
                        LuaExpressionAtomKind.Operator,
                        "*")
                    .Then(Factor)
                    .Many())
            .Before(Parser<LuaExpressionToken>.End);

    public LuaExpressionToken[][]? Match(
        IEnumerable<LuaExpressionToken> input)
        => Matcher.ParseOrNull(
            LuaExpressionTokenSequence.Trim(input));

    public IEnumerable<LuaExpressionToken> Reconstruct(
        LuaExpressionToken[][] form)
        => LuaExpressionTokenSequence.Intercalate(
            form,
            Separator());

    private IEnumerable<LuaExpressionToken> Separator()
    {
        var multiplication = new LuaExpressionAtom(
            "*",
            LuaExpressionAtomKind.Operator);
        if (!options.SpaceAroundOperator)
        {
            return [multiplication];
        }

        return
        [
            new LuaExpressionAtom(
                " ",
                LuaExpressionAtomKind.Whitespace),
            multiplication,
            new LuaExpressionAtom(
                " ",
                LuaExpressionAtomKind.Whitespace),
        ];
    }

    private static bool IsMultiplication(LuaExpressionToken token)
        => token is LuaExpressionAtom
        {
            Kind: LuaExpressionAtomKind.Operator,
            Text: "*",
        };
}
