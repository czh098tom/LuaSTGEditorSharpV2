using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LuaSTGEditorSharpV2.Core.Settings
{
    public static class SettingsHelper
    {
        public static SettingsDescriptor CreateDescriptor(this ISettingsProvider serviceProvider)
        {
            var providerType = serviceProvider.GetType();
            var settingsType = serviceProvider.Settings.GetType();
            return new SettingsDescriptor(
                settingsType,
                providerType,
                serviceProvider);
        }
    }
}
