using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    public class PropertyViewTerm(IServiceProvider serviceProvider, ViewModelProviderServiceProvider viewModelProviderServiceProvider) 
        : IPropertyViewTerm
    {
        [JsonProperty] public NodePropertyCapture? Mapping { get; private set; }
        [JsonProperty] public LocalizableString Caption { get; private set; } = new();
        [JsonProperty] public PropertyViewEditorType? Editor { get; private set; }
        [JsonProperty] public bool Enabled { get; private set; } = true;

        public PropertyItemViewModelBase GetViewModel(NodeData nodeData, PropertyViewContext context)
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData, context);
            return new BasicPropertyItemViewModel(nodeData, context.LocalParam)
            {
                Name = Caption.GetLocalized(),
                Value = Mapping?.Capture(token) ?? string.Empty,
                Type = Editor,
                Enabled = Enabled
            };
        }

        public CommandBase? ResolveCommandOfEditingNode(NodeData nodeData, PropertyViewContext context, string edited)
        {
            return EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider, nodeData, Mapping?.Key, edited);
        }
    }
}
