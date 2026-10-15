using Pidgin;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua;

internal static class PidginParserExtensions
{
    public static TForm? ParseOrNull<TItem, TForm>(
        this Parser<TItem, TForm> parser,
        IEnumerable<TItem> input)
        where TForm : class
    {
        var result = parser.Parse(input);
        return result.Success ? result.Value : null;
    }
}
