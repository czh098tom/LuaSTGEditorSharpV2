using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.ServiceBridge
{
    public class SettingsPageTemplateSelector : ResourceDictKeySelectorBase<object>
    {
        public override bool HasKeyFromSource(object vm)
        {
            return true;
        }

        public override string CreateKey(object vm)
        {
            var service = HostedApplicationHelper.GetService<SettingsDisplayService>();
            if (service.ViewModelMappingInversed.TryGetValue(vm.GetType(), out Type? settingsType))
            {
                if (service.ProviderToDisplay.TryGetValue(settingsType, out var attr))
                {
                    if (!string.IsNullOrEmpty(attr.DisplayKey))
                    {
                        return $"settings_page:{attr.DisplayKey}";
                    }
                    return $"settings_page:null";
                }
            }
            return $"settings_page:null";
        }
    }
}
