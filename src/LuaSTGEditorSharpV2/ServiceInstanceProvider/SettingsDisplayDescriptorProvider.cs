using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.ServiceBridge;
using LuaSTGEditorSharpV2.ServiceBridge.Building.ViewModel;
using LuaSTGEditorSharpV2.ServiceBridge.CodeGenerator.ViewModel;
using LuaSTGEditorSharpV2.ServiceBridge.UICustomization.ViewModel;
using LuaSTGEditorSharpV2.UICustomization;

namespace LuaSTGEditorSharpV2.ServiceInstanceProvider
{
    public class SettingsDisplayDescriptorProvider : IServiceInstanceProvider<SettingsDisplayDescriptor>
    {
        public IReadOnlyCollection<SettingsDisplayDescriptor> GetServiceInstances()
        {
            List<SettingsDisplayDescriptor> arr = 
            [
                new SettingsDisplayDescriptor(typeof(CodeGeneratorServiceProvider), typeof(CodeGenerationServiceSettingsViewModel)),
                new SettingsDisplayDescriptor(typeof(UICustomizationService), typeof(UICustomizationServiceSettingsViewModel)),
                new SettingsDisplayDescriptor(typeof(BuildTaskFactoryServiceProvider), typeof(BuildTaskFactoryServiceSettingsViewModel)),
            ];
            return arr;
        }
    }
}
