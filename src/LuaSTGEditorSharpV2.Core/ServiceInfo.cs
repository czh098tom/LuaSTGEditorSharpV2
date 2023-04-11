using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public record ServiceInfo(string Name, string ShortName, Delegate RegisterFunction)
    {
    }
}
