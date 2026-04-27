namespace LuaSTGEditorSharpV2.Core.Parsing;

/// <summary>
/// Static parser for additive expressions (terms separated by + or -).
/// </summary>
public static class AdditiveParser
{
    /// <summary>
    /// Parses Fragments into additive terms with preserved sign information.
    /// </summary>
    public static List<SignedTerm> Parse(Fragment[] input)
        => Parse(input, ParserOptions.Default);

    public static List<SignedTerm> Parse(Fragment[] input, ParserOptions options)
    {
        var opt = options;
        const char whiteSpace = ' ';
        var terms = new List<SignedTerm>();
        bool contentStarted = false;
        int contentStart = 0;
        int contentEnd = 0;
        Sign currentSign = Sign.Positive;

        for (int i = 0; i < input.Length; i++)
        {
            var fragment = input[i];

            if (fragment.Depth == 0 && (fragment.Character == '+' || fragment.Character == '-'))
            {
                if (contentStarted)
                {
                    terms.Add(new SignedTerm(currentSign, input[contentStart..(contentEnd + 1)]));
                    contentStarted = false;
                }
                currentSign = fragment.Character == '-' ? Sign.Negative : Sign.Positive;
            }
            else if (contentStarted || fragment.Character != whiteSpace)
            {
                if (!contentStarted)
                {
                    contentStarted = true;
                    contentStart = i;
                }
                if (fragment.Character != whiteSpace)
                    contentEnd = i;
            }
        }

        if (contentStarted)
            terms.Add(new SignedTerm(currentSign, input[contentStart..(contentEnd + 1)]));

        return terms;
    }

    /// <summary>
    /// Reconstructs Fragments from terms with operators.
    /// </summary>
    public static Fragment[] Reconstruct(List<SignedTerm> terms)
        => Reconstruct(terms, ParserOptions.Default);

    public static Fragment[] Reconstruct(List<SignedTerm> terms, ParserOptions options)
    {
        var opt = options;
        const char whiteSpace = ' ';
        var result = new List<Fragment>();
        bool isFirst = true;

        foreach (var (sign, term) in terms)
        {
            if (term.Length == 0) continue;

            if (!isFirst || sign == Sign.Negative)
            {
                if (opt.SpaceAroundOperator && !isFirst)
                    result.Add(new Fragment(whiteSpace, 0));
                result.Add(new Fragment(sign == Sign.Negative ? '-' : '+', 0));
                if (opt.SpaceAroundOperator)
                    result.Add(new Fragment(whiteSpace, 0));
            }
            result.AddRange(term);
            isFirst = false;
        }

        return result.ToArray();
    }
}
