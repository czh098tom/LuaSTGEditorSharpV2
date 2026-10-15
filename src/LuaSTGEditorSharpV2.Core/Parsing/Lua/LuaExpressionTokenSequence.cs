namespace LuaSTGEditorSharpV2.Core.Parsing.Lua;

internal static class LuaExpressionTokenSequence
{
    public static LuaExpressionToken[] Trim(
        IEnumerable<LuaExpressionToken> input)
    {
        var tokens = input as LuaExpressionToken[] ?? input.ToArray();
        var start = 0;
        while (start < tokens.Length && IsWhitespace(tokens[start]))
        {
            start++;
        }

        var end = tokens.Length;
        while (end > start && IsWhitespace(tokens[end - 1]))
        {
            end--;
        }

        return start == 0 && end == tokens.Length
            ? tokens
            : tokens[start..end];
    }

    public static string GetText(
        IEnumerable<LuaExpressionToken> input)
        => string.Concat(input.Select(token => token.Text));

    public static bool EquivalentTo(
        IEnumerable<LuaExpressionToken> left,
        IEnumerable<LuaExpressionToken> right)
    {
        var leftTokens = WithoutWhitespace(left);
        var rightTokens = WithoutWhitespace(right);
        return leftTokens.Length == rightTokens.Length
               && leftTokens.Zip(rightTokens).All(pair =>
                   Equivalent(pair.First, pair.Second));
    }

    public static IEnumerable<LuaExpressionToken> Intercalate(
        IEnumerable<IEnumerable<LuaExpressionToken>> input,
        IEnumerable<LuaExpressionToken> separator)
        => input.SelectMany((tokens, index) =>
            index == 0 ? tokens : separator.Concat(tokens));

    internal static bool IsWhitespace(LuaExpressionToken token)
        => token is LuaExpressionAtom
        {
            Kind: LuaExpressionAtomKind.Whitespace,
        };

    private static LuaExpressionToken[] WithoutWhitespace(
        IEnumerable<LuaExpressionToken> input)
        => input.Where(token => !IsWhitespace(token)).ToArray();

    private static bool Equivalent(
        LuaExpressionToken left,
        LuaExpressionToken right)
        => (left, right) switch
        {
            (LuaExpressionGroup leftGroup,
             LuaExpressionGroup rightGroup) =>
                leftGroup.Kind == rightGroup.Kind
                && EquivalentTo(
                    leftGroup.Children,
                    rightGroup.Children),
            (LuaExpressionAtom leftAtom,
             LuaExpressionAtom rightAtom) =>
                leftAtom.Kind == rightAtom.Kind
                && leftAtom.Text == rightAtom.Text,
            _ => false,
        };
}
