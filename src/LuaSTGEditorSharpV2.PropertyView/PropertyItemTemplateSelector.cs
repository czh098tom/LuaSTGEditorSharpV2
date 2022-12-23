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
        public ResourceDictionary? DataTemplates { get; set; }

        public DataTemplate? Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (Default == null) throw new InvalidOperationException($"{nameof(DataTemplates)} has not been assigned");
            if (item is not PropertyViewModel vm) throw new ArgumentException($"{nameof(item)} is not a {nameof(PropertyViewModel)}");
            if (DataTemplates != null && DataTemplates.Contains(vm.Type))
            {
                return (DataTemplate)DataTemplates[vm.Type];
            }
            else
            {
                return Default;
            }
        }
    }
}
