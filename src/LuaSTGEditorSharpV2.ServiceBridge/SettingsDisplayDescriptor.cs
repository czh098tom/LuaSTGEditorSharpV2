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
    public class SettingsDisplayDescriptor(
        Type providerType,
        Type viewModelType,
        SettingsDisplayAttribute displayAttribute,
        IServiceProvider serviceProvider
        ) : PackedDataBase(serviceProvider)
    {

        public Type ProviderType { get; private set; } = providerType;
        public Type ViewModelType { get; private set; } = viewModelType;
        public SettingsDisplayAttribute DisplayAttribute { get; private set; } = displayAttribute;

        public string Name => ProviderType.Name;

        public SettingsDisplayDescriptor(Type providerType, Type viewModelType, IServiceProvider serviceProvider)
            : this(providerType, viewModelType, viewModelType.GetCustomAttribute<SettingsDisplayAttribute>()
                ?? new SettingsDisplayAttribute(), serviceProvider)
        { }
    }
}
