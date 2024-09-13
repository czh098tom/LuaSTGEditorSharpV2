using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    public class SingleListTabTerm<TTermVariable, TIntermediateModel>(IServiceProvider serviceProvider, PropertyViewServiceProvider propertyViewProvider) 
        : PropertyViewTabTermBase(serviceProvider, propertyViewProvider)
        where TTermVariable : class, IMultipleFieldPropertyViewItemTerm<TIntermediateModel>
        where TIntermediateModel : class
    {
        [JsonProperty] public IPropertyViewTerm[] ImmutableProperty { get; private set; } = [];
        [JsonProperty] public PropertyViewTerm? Count { get; private set; } = null;
        [JsonProperty] public TTermVariable? VariableProperty { get; private set; } = null;

        public override PropertyTabViewModel GetPropertyTabViewModel(NodeData nodeData, PropertyViewContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, nodeData, context);
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < ImmutableProperty.Length; i++)
            {
                properties.Add(ImmutableProperty[i].GetViewModel(nodeData, context));
            }
            if (Count != null && VariableProperty != null)
            {
                string countStr = Count.Mapping?.Capture(token) ?? string.Empty;
                int count = 0;
                if (int.TryParse(countStr, out var c))
                {
                    count = c;
                }
                properties.Add(Count.GetViewModel(nodeData, context));
                properties.AddRange(VariableProperty.GetViewModel(nodeData, context, count));
            }
            var tab = new PropertyTabViewModel()
            {
                Caption = Caption?.GetLocalized() ?? PropertyViewServiceProvider.DefaultViewI18NCaption,
            };
            properties.ForEach(tab.Properties.Add);
            return tab;
        }

        public override EditResult ResolveCommandOfEditingNode(NodeData nodeData, PropertyViewContext context, int itemIndex, string edited)
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
                        return new EditResult(VariableProperty?.GetCommand(nodeData, variableDef, idx));
                    }
                    else
                    {
                        return EditResult.Empty;
                    }
                }
                return EditResult.Empty;
            }
            else if (itemIndex == idxCount)
            {
                if (Count != null && VariableProperty != null)
                {
                    return new EditResult(Count.ResolveCommandOfEditingNode(nodeData, context, edited), true);
                }
                return EditResult.Empty;
            }
            else if (itemIndex < idxCount && itemIndex >= 0)
            {
                return new EditResult(ImmutableProperty[itemIndex].ResolveCommandOfEditingNode(nodeData, context, edited), true);
            }
            return EditResult.Empty;
        }
    }
}
