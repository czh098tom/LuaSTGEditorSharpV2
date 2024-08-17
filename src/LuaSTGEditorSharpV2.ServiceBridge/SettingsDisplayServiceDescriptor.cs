using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ServiceBridge
{
    public record class SettingsDisplayServiceDescriptor(
        Type ProviderType,
        Type ViewModelType,
        SettingsDisplayAttribute DisplayAttribute
        )
    {
        public SettingsDisplayServiceDescriptor(Type providerType, Type viewModelType)
            : this(providerType, viewModelType, viewModelType.GetCustomAttribute<SettingsDisplayAttribute>()
                ?? new SettingsDisplayAttribute())
        { }
    }
}
