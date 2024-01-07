using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Settings
{
    public record class SettingsDescriptor(
        string NameKey,
        float SortingOrder,
        Type SettingsType, 
        Type ServiceProviderType, 
        ISettingsProvider SettingsProvider)
    {
    }
}
