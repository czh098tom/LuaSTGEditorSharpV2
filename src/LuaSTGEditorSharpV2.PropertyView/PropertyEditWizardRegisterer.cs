using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.WPF.Services;
using LuaSTGEditorSharpV2.Core.Services;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyEditWizardRegisterer : IServiceInstanceProvider<PropertyEditWizardBase>
    {
        public IReadOnlyCollection<PropertyEditWizardBase> GetServiceInstances(IServiceProvider serviceProvider)
        {
            var arr = new List<PropertyEditWizardBase>
            {
                PropertyEditWizard.Create("file", serviceProvider, (vm, p) =>
                {
                    var editorNodeFactory = serviceProvider.GetRequiredService<EditorNodeFactory>();
                    var localizationService = serviceProvider.GetRequiredService<LocalizationService>();
                    if (serviceProvider.GetRequiredService<FileDialogService>()
                        .ShowOpenFileDialogForSingleFile("property_choose_file", 
                        localizationService.GetString("fileDialog_chooseFileExtension", typeof(PropertyEditWizardRegisterer).Assembly)) is string result)
                    {
                        vm.Value = result;
                        return vm.ResolveEditingNodeCommand(vm.SourceNode, p, result);
                    }
                    return null;
                }),
                PropertyEditWizard.Create("imageFile", serviceProvider, (vm, p) =>
                {
                    var editorNodeFactory = serviceProvider.GetRequiredService<EditorNodeFactory>();
                    var localizationService = serviceProvider.GetRequiredService<LocalizationService>();
                    if (serviceProvider.GetRequiredService<FileDialogService>()
                        .ShowOpenFileDialogForSingleFile("property_choose_file",
                        localizationService.GetString("fileDialog_chooseImageFileExtension", typeof(PropertyEditWizardRegisterer).Assembly)) is string result)
                    {
                        vm.Value = result;
                        return vm.ResolveEditingNodeCommand(vm.SourceNode, p, result);
                    }
                    return null;
                }),
            };

            return arr;
        }
    }
}
