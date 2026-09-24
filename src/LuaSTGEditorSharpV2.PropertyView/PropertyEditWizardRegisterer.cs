using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Dialog.Services;
using LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile;
using LuaSTGEditorSharpV2.PropertyView.Specialized.Vector;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.WPF.Services;

namespace LuaSTGEditorSharpV2.PropertyView;

public class PropertyEditWizardRegisterer : IServiceInstanceProvider<PropertyEditWizardBase>
{
    public IReadOnlyCollection<PropertyEditWizardBase> GetServiceInstances(IServiceProvider serviceProvider)
    {
        var arr = new List<PropertyEditWizardBase>
        {
            PropertyEditWizard.Create("multiplefile", serviceProvider, (viewModel, _) =>
            {
                var viewModelWithCount = (MultipleFileCountPropertyItemViewModel)viewModel;
                if (!viewModelWithCount.CanEdit)
                {
                    return null;
                }
                var localizationService = serviceProvider.GetRequiredService<LocalizationService>();
                var paths = serviceProvider.GetRequiredService<FileDialogService>()
                    .ShowOpenFileDialogForMultipleFile(
                        "property_choose_file",
                        localizationService.GetString(
                            "fileDialog_chooseFileExtension",
                            typeof(PropertyEditWizardRegisterer).Assembly));
                return viewModelWithCount.CreateAppendEdit(paths);
            }),
            PropertyEditWizard.Create("file", serviceProvider, (viewModel, _) =>
            {
                var vm = (BasicPropertyItemViewModel)viewModel;
                var localizationService = serviceProvider.GetRequiredService<LocalizationService>();
                if (serviceProvider.GetRequiredService<FileDialogService>()
                    .ShowOpenFileDialogForSingleFile(
                        "property_choose_file",
                        localizationService.GetString(
                            "fileDialog_chooseFileExtension",
                            typeof(PropertyEditWizardRegisterer).Assembly)) is string result)
                {
                    vm.Value = result;
                }
                return null;
            }),
            PropertyEditWizard.Create("imageFile", serviceProvider, (viewModel, _) =>
            {
                var vm = (BasicPropertyItemViewModel)viewModel;
                var localizationService = serviceProvider.GetRequiredService<LocalizationService>();
                if (serviceProvider.GetRequiredService<FileDialogService>()
                    .ShowOpenFileDialogForSingleFile(
                        "property_choose_file",
                        localizationService.GetString(
                            "fileDialog_chooseImageFileExtension",
                            typeof(PropertyEditWizardRegisterer).Assembly)) is string result)
                {
                    vm.Value = result;
                }
                return null;
            }),
            PropertyEditWizard.Create("code", serviceProvider, (viewModel, _) =>
            {
                var vm = (BasicPropertyItemViewModel)viewModel;
                var editedValue = serviceProvider
                    .GetRequiredService<CodeEditDialogService>()
                    .EditCode(vm.Name, vm.Value);
                if (editedValue is not null)
                {
                    vm.Value = editedValue;
                }
                return null;
            }),
            PropertyEditWizard.Create("multilineText", serviceProvider, (viewModel, _) =>
            {
                var vm = (BasicPropertyItemViewModel)viewModel;
                var editedValue = serviceProvider
                    .GetRequiredService<MultilineTextEditDialogService>()
                    .EditText(vm.Name, vm.Value);
                if (editedValue is not null)
                {
                    vm.Value = editedValue;
                }
                return null;
            }),
            PropertyEditWizard.Create("vector2", serviceProvider, (viewModel, _) =>
            {
                var vm = (Vector2PropertyItemViewModel)viewModel;
                if (serviceProvider
                    .GetRequiredService<Vector2EditDialogService>()
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
