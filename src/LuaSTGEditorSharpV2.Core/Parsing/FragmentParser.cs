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
        var fragments = new Fragment[input.Length];
        uint depth = 0;
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            fragments[i] = c switch
            {
                '(' => new Fragment(c, depth++),
                ')' => new Fragment(c, --depth),
                _ => new Fragment(c, depth)
            };
        }
        return fragments;
    }

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
}
