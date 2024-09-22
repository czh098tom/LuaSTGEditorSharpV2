using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Settings;
using LuaSTGEditorSharpV2.ResourceDictionaryService;

namespace LuaSTGEditorSharpV2.UICustomization
{
    [Inject(ServiceLifetime.Singleton)]
    public class UICustomizationService(ResourceDictionaryRegistrationService resourceDictionaryRegistrationService) : ISettingsProvider
    {
        internal static readonly string uri = "pack://application:,,,/LuaSTGEditorSharpV2.UICustomization;component/UICustomizationStyles.xaml";
        private static readonly string themeKey = "__THEME__";

        public UICustomizationServiceSettings ServiceSettings { get; set; } = new();
        public object Settings
        {
            get => ServiceSettings;
            set
            {
                if (value is not UICustomizationServiceSettings settings) return;
                ServiceSettings = settings;
            }
        }

        private string _currentUri = string.Empty;
        private readonly ResourceDictionary _resourceDictionary = [];

        public void RefreshSettings()
        {
            Update();
        }

        private void Update()
        {
            var settings = ServiceSettings;
            if (settings.Uri != _currentUri)
            {
                _currentUri = settings.Uri;
                _resourceDictionary.Source = new Uri(settings.Uri);
            }

            var fields = settings.GetType()
                .BaseTypes()
                .SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            var properties = settings.GetType()
                .BaseTypes()
                .SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

            foreach (var field in fields)
            {
                var key = field.GetCustomAttribute<ResourceDictionaryKeyAttribute>()?.Key;
                if (!string.IsNullOrEmpty(key))
                {
                    _resourceDictionary.Remove(key);
                    _resourceDictionary.Add(key, field.GetValue(settings));
                }
            }
            foreach (var prop in properties)
            {
                var key = prop.GetCustomAttribute<ResourceDictionaryKeyAttribute>()?.Key;
                if (!string.IsNullOrEmpty(key))
                {
                    _resourceDictionary.Remove(key);
                    _resourceDictionary.Add(key, prop.GetValue(settings));
                }
            }

            resourceDictionaryRegistrationService.Add(themeKey, _resourceDictionary);
        }
    }
}
