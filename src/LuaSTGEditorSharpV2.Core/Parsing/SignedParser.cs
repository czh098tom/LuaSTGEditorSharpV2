namespace LuaSTGEditorSharpV2.Core.Parsing;

/// <summary>
/// Static parser for signed terms (optional leading +/- followed by an atom).
/// </summary>
public static class SignedParser
{
    /// <summary>
    /// Parses a signed expression: optional leading +/- followed by an atom.
    /// </summary>
    public static SignedTerm Parse(Fragment[] input)
        => Parse(input, ParserOptions.Default);

    public static SignedTerm Parse(Fragment[] input, ParserOptions options)
    {
        _ = options;
        const char whiteSpace = ' ';
        int start = 0;
        var sign = Sign.Positive;

        while (start < input.Length && input[start].Character == whiteSpace)
            start++;

        if (start < input.Length && input[start] is { Depth: 0, Character: '+' or '-' })
        {
            sign = input[start].Character == '-' ? Sign.Negative : Sign.Positive;
            start++;
        }

        while (start < input.Length && input[start].Character == whiteSpace)
            start++;

        int end = input.Length - 1;
        while (end >= start && input[end].Character == whiteSpace)
            end--;

        if (end < start)
            return new SignedTerm(sign, []);

        return new SignedTerm(sign, input[start..(end + 1)]);
    }

    /// <summary>
    /// Reconstructs Fragments from signed atom.
    /// </summary>
    public static Fragment[] Reconstruct(SignedTerm term)
        => Reconstruct(term, ParserOptions.Default);

    public static Fragment[] Reconstruct(SignedTerm term, ParserOptions options)
    {
        var opt = options;
        if (term.Term.Length == 0)
            return [];

        var result = new List<Fragment>();

        if (term.Sign == Sign.Negative)
        {
            result.Add(new Fragment('-', 0));
            if (opt.SpaceAroundOperator)
                result.Add(new Fragment(' ', 0));
        }

        result.AddRange(term.Term);
        return result.ToArray();
    }
}
