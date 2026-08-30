using System.Text;

namespace LuaSTGEditorSharpV2.Core.Parsing;

/// <summary>
/// Static parser for expression fragments with depth information.
/// </summary>
public static class FragmentParser
{
    /// <summary>
    /// Parses a string into Fragment array with depth information.
    /// </summary>
    public static Fragment[] Parse(string input)
    {
        ParseCore(input, requireBalancedParentheses: false, out var fragments);
        return fragments;
    }

    /// <summary>
    /// Parses a string into Fragments and rejects unbalanced parentheses.
    /// </summary>
    public static bool TryParse(string input, out Fragment[] fragments)
        => ParseCore(input, requireBalancedParentheses: true, out fragments);

    /// <summary>
    /// Reconstructs a string from Fragment array.
    /// </summary>
    public static string Reconstruct(Fragment[] fragments)
    {
        var sb = new StringBuilder(fragments.Length);
        foreach (var f in fragments)
            sb.Append(f.Character);
        return sb.ToString();
    }

    private static bool ParseCore(
        string input,
        bool requireBalancedParentheses,
        out Fragment[] fragments)
    {
        fragments = new Fragment[input.Length];
        uint depth = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var character = input[i];
            if (character == '(')
            {
                fragments[i] = new Fragment(character, depth++);
            }
            else if (character == ')')
            {
                if (requireBalancedParentheses && depth == 0)
                {
                    fragments = [];
                    return false;
                }

                fragments[i] = new Fragment(character, --depth);
            }
            else
            {
                fragments[i] = new Fragment(character, depth);
            }
        }

        if (requireBalancedParentheses && depth != 0)
        {
            fragments = [];
            return false;
        }

        return true;
    }
}
