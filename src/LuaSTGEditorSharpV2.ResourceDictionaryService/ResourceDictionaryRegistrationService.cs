using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ResourceDictionaryService
{
    [PackedServiceProvider]
    [ServiceShortName("resource")]
    public class ResourceDictionaryRegistrationService(IServiceProvider serviceProvider)
        : PackedDataProviderServiceBase<ResourceDictionaryDescriptor>(serviceProvider)
    {
        private readonly Dictionary<string, ResourceDictionary> _resourceDictionarys = [];

        public IReadOnlyDictionary<string, ResourceDictionary> ResourceDictionarys => _resourceDictionarys;

        protected override void OnActiveServiceAdded(ResourceDictionaryDescriptor newValue)
        {
            base.OnActiveServiceAdded(newValue);
            if (newValue.Uris != null)
            {
                foreach (string? uri in newValue.Uris)
                {
                    if (!string.IsNullOrEmpty(uri))
                    {
                        Add(uri, new()
                        {
                            Source = new Uri(uri)
                        });
                    }
                }
            }
        }

        protected override void OnActiveServiceRemoved(ResourceDictionaryDescriptor oldValue)
        {
            base.OnActiveServiceRemoved(oldValue);
            if (oldValue.Uris != null)
            {
                foreach (string? uri in oldValue.Uris)
                {
                    if (!string.IsNullOrEmpty(uri))
                    {
                        Remove(uri);
                    }
                }
            }
        }

        public void Add(string key, ResourceDictionary resourceDictionary)
        {
            if (_resourceDictionarys.ContainsKey(key)) return;
            _resourceDictionarys.Add(key, resourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        public void Remove(string key)
        {
            if (!_resourceDictionarys.ContainsKey(key)) return;
            var dict = _resourceDictionarys[key];
            _resourceDictionarys.Remove(key);
            Application.Current.Resources.MergedDictionaries.Remove(dict);
        }
    }
}
