using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Resources;

using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyItemTemplateSelector : ResourceDictKeySelectorBase<PropertyItemViewModelBase>
    {
        public override bool HasKeyFromSource(PropertyItemViewModelBase vm)
        {
            return vm.Type != null;
        }

        public override string CreateKey(PropertyItemViewModelBase vm)
        {
            string type = vm.Type!.Name;
            return $"property:{type}";
        }
    }
}
