
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.WPF.Services
{
    [PackagePrimaryKey(nameof(Key))]
    public class ResourceDictionaryDescriptor(string key, Uri dataTemplateResourceDictionaryUri,
        string keyInDictionary, IServiceProvider serviceProvider)
        : PackedDataBase(serviceProvider)
    {
        public string Key { get; private set; } = key;

        public Uri DataTemplateResourceDictionaryUri { get; private set; } = dataTemplateResourceDictionaryUri;

        public string DataTemplateKey { get; private set; } = keyInDictionary;
    }
}
