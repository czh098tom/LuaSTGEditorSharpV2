using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ResourceDictionaryService
{
    public class ResourceDictionaryRegistrationService
    {
        private readonly Dictionary<string, ResourceDictionary> _resourceDictionarys = [];
        public IReadOnlyDictionary<string, ResourceDictionary> ResourceDictionarys => _resourceDictionarys;

        public void Init()
        {
            foreach (string uri in HostedApplicationHelper.GetServices<IResourceProvider>()
                .SelectMany(iprov => iprov.ResourceDictUris))
            {
                Add(uri);
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
