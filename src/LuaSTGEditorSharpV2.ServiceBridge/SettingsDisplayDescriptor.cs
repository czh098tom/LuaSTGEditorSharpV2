using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ServiceBridge
{
    [PackagePrimaryKey(nameof(Name))]
    public record class SettingsDisplayDescriptor(
        Type ProviderType,
        Type ViewModelType,
        SettingsDisplayAttribute DisplayAttribute
        )
    {
        public string Name => ProviderType.Name;

        public SettingsDisplayDescriptor(Type providerType, Type viewModelType)
            : this(providerType, viewModelType, viewModelType.GetCustomAttribute<SettingsDisplayAttribute>()
                ?? new SettingsDisplayAttribute())
        { }
    }
}
