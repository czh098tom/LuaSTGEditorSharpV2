using System.Globalization;
using LuaSTGEditorSharpV2.Core.Parsing.Legacy;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;

internal sealed record LuaSignedTerm(
    Sign Sign,
    LuaExpressionToken[] Term)
{
    public decimal? Value =>
        Term is [LuaExpressionAtom { Kind: LuaExpressionAtomKind.Number } number]
        && decimal.TryParse(number.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? (Sign == LuaSTGEditorSharpV2.Core.Parsing.Legacy.Sign.Negative ? -value : value)
            : null;
}
