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
            var attr = settingsType.GetCustomAttribute<SettingsDisplayAttribute>()
                ?? new SettingsDisplayAttribute(providerType.Name);
            return new SettingsDescriptor(
                attr.Name ?? providerType.Name,
                attr.SortingOrder,
                settingsType,
                providerType,
                serviceProvider);
        }
    }
}
