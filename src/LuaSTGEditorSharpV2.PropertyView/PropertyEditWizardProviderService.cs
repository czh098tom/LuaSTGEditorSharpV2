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
    [PackedServiceProvider]
    public class PropertyEditWizardProviderService(IServiceProvider serviceProvider) 
        : PackedDataProviderServiceBase<PropertyEditWizardBase>(serviceProvider)
    {
        public EditResult? GetEditResult(string key, BasicPropertyItemViewModel viewModel,
            LocalServiceParam localServiceParam)
        {
            var wizard = GetDataOfID(key);
            if (wizard == null) return null;
            return wizard.EditValue(viewModel, localServiceParam);
        }
    }
}
