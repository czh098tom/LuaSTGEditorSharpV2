using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using System.Reflection;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class ConfigurablePropertyViewService : PropertyViewServiceBase
    {
        [JsonProperty]
        public PropertyViewTabTerm[] Tabs { get; private set; } = [];

        protected override IReadOnlyList<PropertyTabViewModel> ResolvePropertyViewModelOfNode(NodeData nodeData
            , PropertyViewContext context)
        {
            List<PropertyTabViewModel> propertyTabViewModels = [];
            for (int i = 0; i < Tabs.Length; i++)
            {
                var mapping = Tabs[i].Mapping;
                List<PropertyItemViewModel> propertyViewModels = new(mapping.Length);

                for (int j = 0; j < mapping.Length; j++)
                {
                    propertyViewModels.Add(new PropertyItemViewModel(
                        mapping[j].LocalizedCaption.GetI18NValueOrDefault(mapping[j].Caption)
                        , nodeData.GetProperty(mapping[j].Mapping), mapping[j].Type));
                }
                var tab = new PropertyTabViewModel()
                {
                    Caption = Tabs[i].LocalizedCaption?.GetI18NValueOrDefault(Tabs[i].Caption
                        ?? DefaultViewI18NCaption) ?? DefaultViewI18NCaption
                };
                propertyViewModels.ForEach(tab.Properties.Add);
                propertyTabViewModels.Add(tab);
            }
            return propertyTabViewModels;
        }

        protected override CommandBase ResolveCommandOfEditingNode(NodeData nodeData, 
            PropertyViewContext context, IReadOnlyList<PropertyTabViewModel> propertyList, 
            int tabIndex, int itemIndex, string edited)
        {
            return EditPropertyCommand.CreateEditCommandOnDemand(nodeData, Tabs[tabIndex].Mapping[itemIndex].Mapping, edited);
        }
    }
}
