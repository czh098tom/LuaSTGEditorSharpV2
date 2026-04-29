namespace LuaSTGEditorSharpV2.Core.Parsing;

/// <summary>
/// Static parser for comma-separated components.
/// </summary>
public static class ComponentParser
{
    /// <summary>
    /// Parses Fragments into components separated by comma at depth 0.
    /// Trims leading and trailing whitespace for each component.
    /// </summary>
    public static Fragment[][] Parse(Fragment[] input)
        => Parse(input, ParserOptions.Default);

    public static Fragment[][] Parse(Fragment[] input, ParserOptions options)
    {
        var opt = options;
        const char whiteSpace = ' ';
        const char separator = ',';
        var components = new List<Fragment[]>();
        bool contentStarted = false;
        int contentStart = 0;
        int contentEnd = 0;

        for (int i = 0; i < input.Length; i++)
        {
            var fragment = input[i];

            if (fragment is { Depth: 0, Character: separator })
            {
                if (contentStarted)
                {
                    components.Add(input[contentStart..(contentEnd + 1)]);
                    contentStarted = false;
                }
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
        {
            components.Add(input[contentStart..(contentEnd + 1)]);
        }

        return components.ToArray();
    }

    /// <summary>
    /// Reconstructs Fragments from components with comma separator.
    /// </summary>
    public static Fragment[] Reconstruct(Fragment[][] components)
        => Reconstruct(components, ParserOptions.Default);

    public static Fragment[] Reconstruct(Fragment[][] components, ParserOptions options)
    {
        var opt = options;

        if (components.Length == 0)
            return [];

        var result = new List<Fragment>();
        for (int i = 0; i < components.Length; i++)
        {
            if (i > 0)
            {
                result.Add(new Fragment(',', 0));
                if (opt.SpaceAfterComma)
                    result.Add(new Fragment(' ', 0));
            }
            result.AddRange(components[i]);
        }
        return result.ToArray();
    }
}
