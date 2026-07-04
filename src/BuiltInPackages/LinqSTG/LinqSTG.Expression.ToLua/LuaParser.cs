using System.Collections.Generic;

namespace LinqSTG.Expression.ToLua
{
    public delegate IEnumerable<LuaCodeLine> LuaParser(IEnumerable<LuaCodeLine> inner);
}
