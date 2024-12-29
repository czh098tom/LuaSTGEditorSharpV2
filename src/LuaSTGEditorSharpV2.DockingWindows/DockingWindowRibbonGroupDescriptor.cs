using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.WPF.Services;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    public class DockingWindowRibbonGroupDescriptor(string key, Uri dataTemplateResourceDictionaryUri, 
        string keyInDictionary, int priority, IServiceProvider serviceProvider) 
        : ResourceDictionaryDescriptor(key, dataTemplateResourceDictionaryUri, keyInDictionary, 
            serviceProvider)
    {
        public int Priority { get; private set; } = priority;
    }
}
