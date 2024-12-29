using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.WPF.Services;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    [PackagePrimaryKey(nameof(Key))]
    public class DockingWindowDescriptor(Type viewModelType, Uri dataTemplateResourceDictionaryUri, 
        string dataTemplateKey, IServiceProvider serviceProvider) 
        : ResourceDictionaryDescriptor(viewModelType.Name, dataTemplateResourceDictionaryUri, 
            dataTemplateKey, serviceProvider)
    {
        public Type ViewModelType { get; private set; } = viewModelType;
    }
}
