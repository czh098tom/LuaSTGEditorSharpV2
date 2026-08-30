using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [JsonTypeShortName(typeof(PropertyTabTermBase), "Default")]
    public class CommonPropertyTabTerm(IServiceProvider serviceProvider, PropertyViewServiceProvider propertyViewServiceProvider) 
        : PropertyTabTermBase(serviceProvider, propertyViewServiceProvider), IMultiSourcePropertyTabTerm
    {
        [JsonProperty] public IPropertyItemTerm[] Mapping { get; set; } = [];

        public override PropertyTabViewModel GetPropertyTabViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            var mapping = Mapping;
            List<PropertyItemViewModelBase> propertyViewModels = new(mapping.Length);

            for (int j = 0; j < mapping.Length; j++)
            {
                propertyViewModels.Add(mapping[j].GetViewModel(nodeData, context));
            }
            var tab = new PropertyTabViewModel()
            {
                Caption = Caption?.GetLocalized() ?? PropertyViewServiceProvider.DefaultViewI18NCaption
            };
            propertyViewModels.ForEach(tab.Properties.Add);
            return tab;
        }

        public PropertyTabViewModel GetPropertyTabViewModel(
            IReadOnlyList<EditorNode> nodeData,
            PropertyViewContext context)
        {
            List<PropertyItemViewModelBase> propertyViewModels = new(Mapping.Length);
            foreach (var term in Mapping)
            {
                if (term is IMultiSourcePropertyItemTerm multiSourceTerm)
                {
                    propertyViewModels.Add(multiSourceTerm.GetViewModel(nodeData, context));
                    continue;
                }

                var placeholder = new UnsupportedMultiSourcePropertyItemViewModel(
                    PropertyViewServiceProvider.MultiSelectionUnsupportedI18NText);
                placeholder.Initialize(
                    nodeData,
                    context.LocalParam,
                    ServiceProvider.GetRequiredService<PropertyEditWizardProviderService>());
                propertyViewModels.Add(placeholder);
            }

            var tab = new PropertyTabViewModel(true)
            {
                Caption = Caption?.GetLocalized() ?? PropertyViewServiceProvider.DefaultViewI18NCaption
            };
            propertyViewModels.ForEach(tab.Properties.Add);
            return tab;
        }
    }
}
