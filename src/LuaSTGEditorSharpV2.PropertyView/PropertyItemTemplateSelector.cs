using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Resources;

using LuaSTGEditorSharpV2.Core.ViewModel;

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
            if (dataTemplates != null && vm.Type != null && dataTemplates.Contains(vm.Type))
            {
                return (DataTemplate)dataTemplates[$"property:{vm.Type}"];
            }
            else
            {
                return Default;
            }
        }
    }
}
