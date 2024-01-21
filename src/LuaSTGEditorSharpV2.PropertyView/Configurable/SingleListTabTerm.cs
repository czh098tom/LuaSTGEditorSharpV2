using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class SingleListTabTerm<TTermVariable, TIntermediateModel> : PropertyViewTabTermBase
        where TTermVariable : class, IMultipleFieldPropertyViewItemTerm<TIntermediateModel>
        where TIntermediateModel : class
    {
        [JsonProperty] public PropertyViewTerm[] ImmutableProperty { get; private set; } = [];
        [JsonProperty] public PropertyViewTerm? Count { get; private set; } = null;
        [JsonProperty] public TTermVariable? VariableProperty { get; private set; } = null;

        public override PropertyTabViewModel GetPropertyTabViewModel(NodeData nodeData)
        {
            List<PropertyItemViewModelBase> properties = [];
            if (Count != null && VariableProperty != null)
            {
                string countStr = Count.Mapping?.Capture(nodeData) ?? string.Empty;
                int count = 0;
                if (int.TryParse(countStr, out var c))
                {
                    count = c;
                }
                properties.Add(Count.GetViewModel(nodeData));
                properties.AddRange(VariableProperty.GetViewModel(nodeData, count));
            }
            var tab = new PropertyTabViewModel()
            {
                Caption = Caption?.GetLocalized() ?? PropertyViewServiceProvider.DefaultViewI18NCaption,
            };
            properties.ForEach(tab.Properties.Add);
            return tab;
        }

        public override CommandBase? ResolveCommandOfEditingNode(NodeData nodeData, PropertyViewContext context, int itemIndex, string edited)
        {
            int idxCount = ImmutableProperty.Length;
            if (itemIndex > idxCount)
            {
                if (Count != null && VariableProperty != null)
                {
                    var idx = itemIndex - idxCount - 1;
                    TIntermediateModel? variableDef = null;
                    try
                    {
                        variableDef = JsonConvert.DeserializeObject<TIntermediateModel>(edited);
                    }
                    catch (Exception) { }
                    if (variableDef != null)
                    {
                        return VariableProperty?.GetCommand(nodeData, variableDef, idx);
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
            }
            else if (itemIndex == idxCount)
            {
                if (Count != null && VariableProperty != null)
                {
                    return EditPropertyCommand.CreateEditCommandOnDemand(nodeData, Count.Mapping?.Key, edited);
                }
                return null;
            }
            else if (itemIndex < idxCount && itemIndex > 0)
            {
                return EditPropertyCommand.CreateEditCommandOnDemand(nodeData, ImmutableProperty[itemIndex].Mapping?.Key, edited);
            }
            return null;
        }
    }
}
