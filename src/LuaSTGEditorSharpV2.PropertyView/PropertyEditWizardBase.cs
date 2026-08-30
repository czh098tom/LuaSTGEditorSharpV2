using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [PackagePrimaryKey(nameof(Name))]
    public abstract class PropertyEditWizardBase(string name, IServiceProvider serviceProvider) 
        : PackedDataBase(serviceProvider)
    {
        public string Name { get; } = name;

        public abstract EditResult? EditValue(PropertyItemViewModelBase viewModel,
            LocalServiceParam localServiceParam);
    }
}
