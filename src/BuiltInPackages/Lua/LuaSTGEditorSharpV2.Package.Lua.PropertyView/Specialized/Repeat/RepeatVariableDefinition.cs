using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat
{
    public record class RepeatVariableDefinition(
               string Name,
               string Init,
               string Increment)
    {
    }
}
