using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    internal record ServiceInfo(string Name, string ShortName
        , Type ContextType, Type SettingsType
        , Delegate RegisterFunction, Delegate SettingsReplacementFunction)
    {
    }
}
