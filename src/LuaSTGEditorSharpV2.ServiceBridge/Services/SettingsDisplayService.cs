using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Core.Settings;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.ServiceBridge.ViewModel;
using LuaSTGEditorSharpV2.ResourceDictionaryService;

namespace LuaSTGEditorSharpV2.ServiceBridge.Services
{
    public class SettingsDisplayService(ILogger<SettingsDisplayService> logger)
    {
        private readonly ILogger<SettingsDisplayService> _logger = logger;

        private readonly Dictionary<Type, Type> _viewModelMapping = [];
        public IReadOnlyDictionary<Type, Type> ViewModelMapping => _viewModelMapping;

        private readonly Dictionary<Type, Type> _viewModelMappingInversed = [];
        public IReadOnlyDictionary<Type, Type> ViewModelMappingInversed => _viewModelMappingInversed;

        private readonly Dictionary<Type, SettingsDisplayAttribute> _providerToDisplay = [];
        public IReadOnlyDictionary<Type, SettingsDisplayAttribute> ProviderToDisplay => _providerToDisplay;

        public void RegisterViewModel<TProvider, TViewModel>()
            where TProvider : ISettingsProvider
            where TViewModel : ViewModelBase
        {
            RegisterViewModel(typeof(TProvider), typeof(TViewModel));
        }

        public IReadOnlyList<SettingsPageViewModel> MapViewModel()
        {
            var settingsService = HostedApplicationHelper.GetService<SettingsService>();
            var localizationService = HostedApplicationHelper.GetService<LocalizationService>();
            List<SettingsPageViewModel> viewModels = new(settingsService.SettingsDescriptors.Count);
            var descriptors = new List<SettingsDescriptor>(settingsService.SettingsDescriptors
                .OrderBy(desc => _providerToDisplay.GetValueOrDefault(desc.ServiceProviderType)?.SortingOrder ?? 0));
            for (int i = 0; i < settingsService.SettingsDescriptors.Count; i++)
            {
                var desc = settingsService.SettingsDescriptors[i];
                if (_viewModelMapping.TryGetValue(desc.ServiceProviderType, out var viewModelType))
                {
                    var attr = _providerToDisplay.GetValueOrDefault(desc.ServiceProviderType);
                    try
                    {
                        var viewModel = JsonConvert.DeserializeObject(
                                JsonConvert.SerializeObject(desc.SettingsProvider.Settings),
                                viewModelType)
                            ?? new object();
                        var pageVm = new SettingsPageViewModel()
                        {
                            Title = localizationService.GetString(attr?.Name ?? desc.SettingsType.Name, viewModelType.Assembly)
                        };
                        pageVm.PageItems.Add(viewModel);
                        viewModels.Add(pageVm);
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e);
                        _logger.LogError("Converting from \"{settings_type}\" to \"{settings_viewmodel_type}\" failed.",
                            desc.ServiceProviderType.Name, viewModelType.Name);
                    }
                }
            }
            return viewModels;
        }

        public void WriteViewModelBack(IReadOnlyList<SettingsPageViewModel> viewModels)
        {
            for (int i = 0; i < viewModels.Count; i++)
            {
                var vm = viewModels[i];
                var pageItem = vm.PageItems[0];
                if (_viewModelMappingInversed.TryGetValue(pageItem.GetType(), out var providerType))
                {
                    try
                    {
                        if (HostedApplicationHelper.GetService(providerType) is ISettingsProvider provider)
                        {
                            provider.Settings = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(pageItem),
                                provider.Settings.GetType()) ?? throw new InvalidOperationException();
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e);
                        _logger.LogError("Writting from \"{settings_viewmodel_type}\" to \"{settings_provider_type}\" failed.",
                            vm.GetType().Name, providerType);
                    }
                }
            }
        }

        private void RegisterViewModel(Type providerType, Type viewModelType)
        {
            _viewModelMapping.Add(providerType, viewModelType);
            _viewModelMappingInversed.Add(viewModelType, providerType);
            _providerToDisplay.Add(providerType, viewModelType.GetCustomAttribute<SettingsDisplayAttribute>()
                ?? new SettingsDisplayAttribute());
        }
    }
}
