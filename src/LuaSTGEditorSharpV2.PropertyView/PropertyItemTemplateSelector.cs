using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Resources;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ResourceDictionary dataTemplates = Application.Current.Resources;
            if (Default == null) throw new InvalidOperationException($"{nameof(dataTemplates)} has not been assigned");
            if (item is not PropertyViewModel vm) throw new ArgumentException($"{nameof(item)} is not a {nameof(PropertyViewModel)}");
            if (dataTemplates != null && vm.Type != null)
            {
                string key = $"property:{vm.Type}";
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
    }
}
