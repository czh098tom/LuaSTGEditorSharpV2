using LuaSTGEditorSharpV2.Core.Parsing.Legacy;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;

internal sealed class AdditiveTermsPattern(
    ParserOptions options)
    : IPattern<LuaExpressionToken, List<LuaSignedTerm>>
{
    private static readonly Parser<
        LuaExpressionToken,
        LuaExpressionToken[]> Term =
            Parser<LuaExpressionToken>.Token(token => !IsSign(token))
                .Many()
                .Capture()
                .Map(LuaExpressionTokenSequence.Trim);

    private static readonly Parser<LuaExpressionToken, Sign> SignParser =
        LuaExpressionTokenParsers
            .Atom(LuaExpressionAtomKind.Operator)
            .Assert(token => token.Text is "+" or "-")
            .Map(token => token.Text == "-"
                ? Sign.Negative
                : Sign.Positive);

    private static readonly Parser<LuaExpressionToken, Sign> SignRun =
        Parser.Map(
            (first, remaining) =>
                remaining.LastOrDefault(first),
            SignParser,
            Parser.Try(
                LuaExpressionTokenParsers.SkipWhitespace
                    .Then(SignParser))
                .Many());

    private static readonly Parser<
        LuaExpressionToken,
        ParsedTerm> FollowingTerm =
            Parser.Map(
                (sign, tokens) =>
                    new ParsedTerm(sign, tokens),
                SignRun,
                Term);

    private static readonly Parser<
        LuaExpressionToken,
        List<LuaSignedTerm>> Matcher =
            Parser.Map(
                (first,
remaining) =>
                    remaining
                        .Prepend(new ParsedTerm(
                            Sign.Positive,
                            first))
                        .Where(term => term.Tokens.Length > 0)
                        .Select(term => new LuaSignedTerm(
                            term.Sign,
                            term.Tokens))
                        .ToList(),
                Term,
                FollowingTerm.Many())
            .Before(Parser<LuaExpressionToken>.End);

    public List<LuaSignedTerm>? Match(
        IEnumerable<LuaExpressionToken> input)
        => Matcher.ParseOrNull(
            LuaExpressionTokenSequence.Trim(input));

    public IEnumerable<LuaExpressionToken> Reconstruct(
        List<LuaSignedTerm> form)
        => form
            .Where(term => term.Term.Length > 0)
            .SelectMany((term, index) =>
                Prefix(index, term.Sign).Concat(term.Term));

    private IEnumerable<LuaExpressionToken> Prefix(
        int index,
        Sign sign)
    {
        if (index == 0 && sign == Sign.Positive)
        {
            return [];
        }

        var symbol = new LuaExpressionAtom(
            sign == Sign.Negative ? "-" : "+",
            LuaExpressionAtomKind.Operator);
        var whitespace = new LuaExpressionAtom(
            " ",
            LuaExpressionAtomKind.Whitespace);

        if (!options.SpaceAroundOperator)
        {
            return [symbol];
        }

        return index == 0
            ? [symbol, whitespace]
            : [whitespace, symbol, whitespace];
    }

    private static bool IsSign(LuaExpressionToken token)
        => token is LuaExpressionAtom
        {
            Kind: LuaExpressionAtomKind.Operator,
            Text: "+" or "-",
        };

    private sealed record ParsedTerm(
        Sign Sign,
        LuaExpressionToken[] Tokens);
}
