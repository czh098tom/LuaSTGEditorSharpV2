using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyEditWizard(string name,
        IServiceProvider serviceProvider,
        Func<PropertyItemViewModelBase, LocalServiceParam, EditResult?> edit)
        : PropertyEditWizardBase(name, serviceProvider)
    {
        public static PropertyEditWizard Create(string name,
            IServiceProvider serviceProvider,
            Func<PropertyItemViewModelBase, LocalServiceParam, EditResult?> edit)
        {
            return new PropertyEditWizard(name, serviceProvider, edit);
        }

        public override EditResult? EditValue(PropertyItemViewModelBase viewModel, LocalServiceParam localServiceParam)
        {
            return edit.Invoke(viewModel, localServiceParam);
        }
    }
}
