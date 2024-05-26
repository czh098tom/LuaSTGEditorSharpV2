using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LuaSTGEditorSharpV2.WPF
{
    public abstract class ResourceDictKeySelectorBase<TViewModel> : DataTemplateSelector
        where TViewModel : class
    {
        public DataTemplate? Default { get; set; }

        public override sealed DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ResourceDictionary dataTemplates = GetResourceDictionary();
            if (Default == null) throw new InvalidOperationException($"{nameof(dataTemplates)} has not been assigned");
            if (item == null) return Default;
            if (item is not TViewModel vm) throw new ArgumentException($"{nameof(item)} is not a {typeof(TViewModel)}");
            if (dataTemplates != null && HasKeyFromSource(vm))
            {
                string key = CreateKey(vm);
                if (dataTemplates.Contains(key))
                {
                    return (DataTemplate)dataTemplates[key];
                }
                else
                {
                    return Default;
                }
            }
            else
            {
                return Default;
            }
        }

        public abstract ResourceDictionary GetResourceDictionary();
        public abstract bool HasKeyFromSource(TViewModel vm);
        public abstract string CreateKey(TViewModel vm);
    }
}
