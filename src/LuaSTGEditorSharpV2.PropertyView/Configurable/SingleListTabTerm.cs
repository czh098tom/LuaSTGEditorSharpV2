using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    public class SingleListTabTerm<TTermVariable, TIntermediateModel>(IServiceProvider serviceProvider, PropertyViewServiceProvider propertyViewProvider) 
        : PropertyTabTermBase(serviceProvider, propertyViewProvider)
        where TTermVariable : class, IMultipleFieldPropertyItemTerm<TIntermediateModel>
        where TIntermediateModel : class
    {
        [JsonProperty] public IPropertyItemTerm[] ImmutableProperty { get; private set; } = [];
        [JsonProperty] public PropertyItemTerm? Count { get; private set; } = null;
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
    }
}
