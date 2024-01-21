using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LuaSTGEditorSharpV2.WPF
{
    public abstract class ResourceDictKeySelector<TViewModel> : ResourceDictKeySelectorBase<TViewModel>
        where TViewModel : class
    {
        public ResourceDictionary ResourceDictionary { get; set; } = [];

        public override ResourceDictionary GetResourceDictionary()
        {
            return ResourceDictionary;
        }
    }
}
