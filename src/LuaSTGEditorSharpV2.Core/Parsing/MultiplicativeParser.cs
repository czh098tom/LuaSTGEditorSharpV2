namespace LuaSTGEditorSharpV2.Core.Parsing;

/// <summary>
/// Splits expressions into factors separated by top-level multiplication signs.
/// </summary>
public static class MultiplicativeParser
{
    public static Fragment[][] Parse(Fragment[] input)
        => Parse(input, ParserOptions.Default);

    public static Fragment[][] Parse(Fragment[] input, ParserOptions options)
    {
        _ = options;
        if (input.Length == 0)
        {
            return [];
        }

        var factors = new List<Fragment[]>();
        var start = 0;
        var hasSeparator = false;

        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] is not { Depth: 0, Character: '*' })
            {
                continue;
            }

            factors.Add(Trim(input[start..i]));
            start = i + 1;
            hasSeparator = true;
        }

        var finalFactor = Trim(input[start..]);
        if (hasSeparator || finalFactor.Length > 0)
        {
            factors.Add(finalFactor);
        }

        return factors.ToArray();
    }

    public static Fragment[] Reconstruct(Fragment[][] factors)
        => Reconstruct(factors, ParserOptions.Default);

    public static Fragment[] Reconstruct(Fragment[][] factors, ParserOptions options)
    {
        if (factors.Length == 0)
        {
            return [];
        }

        var result = new List<Fragment>();
        for (var i = 0; i < factors.Length; i++)
        {
            if (i > 0)
            {
                if (options.SpaceAroundOperator)
                {
                    result.Add(new Fragment(' ', 0));
                }

                result.Add(new Fragment('*', 0));

                if (options.SpaceAroundOperator)
                {
                    result.Add(new Fragment(' ', 0));
                }
            }

            result.AddRange(factors[i]);
        }

        return result.ToArray();
    }

    private static Fragment[] Trim(Fragment[] fragments)
    {
        var start = 0;
        while (start < fragments.Length && char.IsWhiteSpace(fragments[start].Character))
        {
            start++;
        }

        var end = fragments.Length - 1;
        while (end >= start && char.IsWhiteSpace(fragments[end].Character))
        {
            end--;
        }

        return end < start ? [] : fragments[start..(end + 1)];
    }
}
