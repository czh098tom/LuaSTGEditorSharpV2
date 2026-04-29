namespace LuaSTGEditorSharpV2.Core.Parsing.Facade;

/// <summary>
/// Helper for Vector2 expression editing.
/// </summary>
public static class Vector2EditHelper
{
    public static (string, string) Decompose(string expression)
    {
        var fragments = FragmentParser.Parse(expression);
        var comps = ComponentParser.Parse(fragments)
            .Select(FragmentParser.Reconstruct)
            .ToArray();
        if (comps.Length == 0) return (string.Empty, string.Empty);
        else if (comps.Length == 1) return (comps[0], string.Empty);
        return (comps[0], comps[1]);
    }

    public static string Compose(string xExpression, string yExpression)
    {
        string[] comps = [xExpression, yExpression];
        return FragmentParser.Reconstruct(
            ComponentParser.Reconstruct(comps.Select(FragmentParser.Parse).ToArray()));
    }
}
