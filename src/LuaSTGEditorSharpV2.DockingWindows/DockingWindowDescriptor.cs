using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    [PackagePrimaryKey(nameof(Key))]
    public class DockingWindowDescriptor(Type viewModelType, Uri dataTemplateResourceDictionaryUri, 
        string dataTemplateKey, IServiceProvider serviceProvider) 
        : PackedDataBase(serviceProvider)
    {
        public string Key { get; private set; } = viewModelType.Name;

        public Type ViewModelType { get; private set; } = viewModelType;

        public Uri DataTemplateResourceDictionaryUri { get; private set; } = dataTemplateResourceDictionaryUri;

        public string DataTemplateKey { get; private set; } = dataTemplateKey;
    }
}
