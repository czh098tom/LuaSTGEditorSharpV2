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
    [Inject(lifetime: ServiceLifetime.Singleton)]
    public class UICustomizationService : ISettingsProvider
    {
        internal static readonly string uri = "pack://application:,,,/LuaSTGEditorSharpV2.UICustomization;component/UICustomizationStyles.xaml";

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

        public IReadOnlyList<string> ResourceDictUris => _resourceDictUris;

        private readonly List<string> _resourceDictUris = [string.Empty];

        private string currentUri = string.Empty;

        public void RefreshSettings()
        {
            Update();
        }

        private void Update()
        {
            SetSource(ServiceSettings.Uri);
            var settings = ServiceSettings;
            var fields = settings.GetType()
                .BaseTypes()
                .SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            var properties = settings.GetType()
                .BaseTypes()
                .SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            var service = HostedApplicationHelper.GetService<ResourceDictionaryRegistrationService>();
            var dict = service.ResourceDictionarys[currentUri];
            if (dict == null) return;
            foreach (var field in fields)
            {
                var key = field.GetCustomAttribute<ResourceDictionaryKeyAttribute>()?.Key;
                if (!string.IsNullOrEmpty(key))
                {
                    dict[key] = field.GetValue(settings);
                }
            }
            foreach (var prop in properties)
            {
                var key = prop.GetCustomAttribute<ResourceDictionaryKeyAttribute>()?.Key;
                if (!string.IsNullOrEmpty(key))
                {
                    dict[key] = prop.GetValue(settings);
                }
            }
        }

        private void SetSource(string uri)
        {
            if (uri != currentUri)
            {
                var service = HostedApplicationHelper.GetService<ResourceDictionaryRegistrationService>();
                service.Remove(currentUri);
                currentUri = uri;
                _resourceDictUris[0] = uri;
                service.Add(uri);
            }
        }
    }
}
