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
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.PropertyView.Specialized.Vector;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyEditWizardRegisterer : IServiceInstanceProvider<PropertyEditWizardBase>
    {
        public IReadOnlyCollection<PropertyEditWizardBase> GetServiceInstances(IServiceProvider serviceProvider)
        {
            var arr = new List<PropertyEditWizardBase>
            {
                PropertyEditWizard.Create("file", serviceProvider, (PropertyItemViewModelBase viewModel, LocalServiceParam p) =>
                {
                    var vm = (BasicPropertyItemViewModel)viewModel;
                    var editorNodeFactory = serviceProvider.GetRequiredService<EditorNodeFactory>();
                    var localizationService = serviceProvider.GetRequiredService<LocalizationService>();
                    if (serviceProvider.GetRequiredService<FileDialogService>()
                        .ShowOpenFileDialogForSingleFile("property_choose_file", 
                        localizationService.GetString("fileDialog_chooseFileExtension", typeof(PropertyEditWizardRegisterer).Assembly)) is string result)
                    {
                        vm.Value = result;
                    }
                    return null;
                }),
                PropertyEditWizard.Create("imageFile", serviceProvider, (PropertyItemViewModelBase viewModel, LocalServiceParam p) =>
                {
                    var vm = (BasicPropertyItemViewModel)viewModel;
                    var editorNodeFactory = serviceProvider.GetRequiredService<EditorNodeFactory>();
                    var localizationService = serviceProvider.GetRequiredService<LocalizationService>();
                    if (serviceProvider.GetRequiredService<FileDialogService>()
                        .ShowOpenFileDialogForSingleFile("property_choose_file",
                        localizationService.GetString("fileDialog_chooseImageFileExtension", typeof(PropertyEditWizardRegisterer).Assembly)) is string result)
                    {
                        vm.Value = result;
                    }
                    return null;
                }),
                PropertyEditWizard.Create("code", serviceProvider, (PropertyItemViewModelBase viewModel, LocalServiceParam p) =>
                {
                    var vm = (BasicPropertyItemViewModel)viewModel;
                    var editedValue = serviceProvider
                        .GetRequiredService<ICodeEditDialogService>()
                        .EditCode(vm.Name, vm.Value);
                    if (editedValue is not null)
                    {
                        vm.Value = editedValue;
                    }
                    return null;
                }),
                PropertyEditWizard.Create("multilineText", serviceProvider, (PropertyItemViewModelBase viewModel, LocalServiceParam p) =>
                {
                    var vm = (BasicPropertyItemViewModel)viewModel;
                    var editedValue = serviceProvider
                        .GetRequiredService<IMultilineTextEditDialogService>()
                        .EditText(vm.Name, vm.Value);
                    if (editedValue is not null)
                    {
                        vm.Value = editedValue;
                    }
                    return null;
                }),
                PropertyEditWizard.Create("vector2", serviceProvider, (PropertyItemViewModelBase viewModel, LocalServiceParam p) =>
                {
                    var vm = (Vector2PropertyItemViewModel)viewModel;
                    if (serviceProvider.GetRequiredService<IVector2EditDialogService>()
                        .EditVector2(vm.Name, vm.X, vm.Y) is { } edit)
                    {
                        return vm.ApplyVector2Edit(edit.X, edit.Y);
                    }
                    return null;
                }),
            };

            return arr;
        }
    }
}
