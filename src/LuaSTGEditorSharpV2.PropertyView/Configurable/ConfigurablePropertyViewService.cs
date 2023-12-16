using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class ConfigurablePropertyViewService : PropertyViewServiceBase
    {
        [JsonProperty]
        public PropertyViewTabTermBase[] Tabs { get; private set; } = [];

        internal protected override IReadOnlyList<PropertyTabViewModel> ResolvePropertyViewModelOfNode(NodeData nodeData
            , PropertyViewContext context)
        {
            List<PropertyTabViewModel> propertyTabViewModels = [];
            for (int i = 0; i < Tabs.Length; i++)
            {
                propertyTabViewModels.Add(Tabs[i].GetPropertyTabViewModel(nodeData));
            }
            return propertyTabViewModels;
        }

        internal protected override CommandBase? ResolveCommandOfEditingNode(NodeData nodeData, 
            PropertyViewContext context, IReadOnlyList<PropertyTabViewModel> propertyList, 
            int tabIndex, int itemIndex, string edited)
        {
            if (tabIndex < 0 || tabIndex >= Tabs.Length) return null;
            return Tabs[tabIndex].ResolveCommandOfEditingNode(nodeData, context, itemIndex, edited);
        }
    }
}
