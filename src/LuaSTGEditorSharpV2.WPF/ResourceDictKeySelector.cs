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
            ResourceDictionary dataTemplates = Application.Current.Resources;
            if (Default == null) throw new InvalidOperationException($"{nameof(dataTemplates)} has not been assigned");
            if (item is not TViewModel vm) throw new ArgumentException($"{nameof(item)} is not a {nameof(TViewModel)}");
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

        public abstract bool HasKeyFromSource(TViewModel vm);
        public abstract string CreateKey(TViewModel vm);
    }
}
