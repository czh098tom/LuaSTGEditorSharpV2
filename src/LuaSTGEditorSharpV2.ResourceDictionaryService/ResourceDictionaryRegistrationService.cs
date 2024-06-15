using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ResourceDictionaryService
{
    [ServiceShortName("resource")]
    public class ResourceDictionaryRegistrationService : PackedDataProviderServiceBase<ResourceDictionaryDescriptor>
    {
        private readonly Dictionary<string, ResourceDictionary> _resourceDictionarys = [];
        public IReadOnlyDictionary<string, ResourceDictionary> ResourceDictionarys => _resourceDictionarys;

        protected override void OnRegistered(string id, PackageInfo packageInfo, ResourceDictionaryDescriptor data)
        {
            base.OnRegistered(id, packageInfo, data);
            if (data.Uris != null)
            {
                foreach (string? uri in data.Uris)
                {
                    if (!string.IsNullOrEmpty(uri))
                    {
                        Add(uri);
                    }
                }
            }
        }

        protected override void OnUnloaded(string id, PackageInfo packageInfo, ResourceDictionaryDescriptor data)
        {
            base.OnUnloaded(id, packageInfo, data);
            if (data.Uris != null)
            {
                foreach (string? uri in data.Uris)
                {
                    if (!string.IsNullOrEmpty(uri))
                    {
                        Remove(uri);
                    }
                }
            }
        }

        public void Add(string uri)
        {
            if (_resourceDictionarys.ContainsKey(uri)) return;
            ResourceDictionary dictionary = new()
            {
                Source = new Uri(uri)
            };
            _resourceDictionarys.Add(uri, dictionary);
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }

        public void Remove(string uri)
        {
            if (!_resourceDictionarys.ContainsKey(uri)) return;
            var dict = _resourceDictionarys[uri];
            _resourceDictionarys.Remove(uri);
            Application.Current.Resources.MergedDictionaries.Remove(dict);
        }
    }
}
