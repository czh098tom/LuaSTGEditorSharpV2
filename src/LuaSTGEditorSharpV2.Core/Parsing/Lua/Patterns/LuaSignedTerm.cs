using LuaSTGEditorSharpV2.Core.Parsing.Legacy;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;

namespace LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;

internal sealed record LuaSignedTerm(
    Sign Sign,
    LuaExpressionToken[] Term);
