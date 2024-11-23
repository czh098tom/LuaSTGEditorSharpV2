using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.WPF.Services;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    public class DockingWindowRibbonButtonDescriptor(string key, Uri dataTemplateResourceDictionaryUri, 
        string keyInDictionary, string groupKey, Type anchorableViewModelType, IServiceProvider serviceProvider) 
        : ResourceDictionaryDescriptor(key, dataTemplateResourceDictionaryUri, 
            keyInDictionary, serviceProvider)
    {
        public string GroupKey { get; private set; } = groupKey;

        public Type AnchorableViewModelType { get; private set; } = anchorableViewModelType;
    }
}
