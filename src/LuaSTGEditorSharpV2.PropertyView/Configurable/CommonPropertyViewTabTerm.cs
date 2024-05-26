using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class CommonPropertyViewTabTerm : PropertyViewTabTermBase
    {
        [JsonProperty] public IPropertyViewTerm[] Mapping { get; set; } = [];

        public override PropertyTabViewModel GetPropertyTabViewModel(NodeData nodeData, PropertyViewContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
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

        public override EditResult ResolveCommandOfEditingNode(NodeData nodeData,
            PropertyViewContext context, int itemIndex, string edited)
        {
            if (itemIndex < 0 || itemIndex >= Mapping.Length) return EditResult.Empty;
            return new EditResult(Mapping[itemIndex].ResolveCommandOfEditingNode(nodeData, context, edited), false);
        }
    }
}
