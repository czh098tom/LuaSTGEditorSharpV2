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
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    [JsonUseShortNaming]
    [JsonTypeShortName(typeof(IPropertyItemTerm), "Default")]
    public class PropertyItemTerm(IServiceProvider serviceProvider)
        : IPropertyItemTerm
    {
        [JsonProperty] public NodePropertyCapture? Mapping { get; private set; }
        [JsonProperty] public LocalizableString Caption { get; private set; } = new();
        [JsonProperty] public PropertyViewEditorType? Editor { get; protected set; }
        [JsonProperty] public bool Enabled { get; private set; } = true;

        public virtual PropertyItemViewModelBase GetViewModel(NodeData nodeData, PropertyViewContext context)
        {
            return GetViewModelImpl<BasicPropertyItemViewModelFactory, BasicPropertyItemViewModel>(nodeData, context);
        }

        protected PropertyItemViewModelBase GetViewModelImpl<TFactory, TResult>(NodeData nodeData, PropertyViewContext context)
            where TFactory : IBasicPropertyItemViewModelFactory<TResult>
            where TResult : BasicPropertyItemViewModel
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData, context);
            var vm = serviceProvider.GetRequiredService<TFactory>()
                .Create(nodeData, context.LocalParam, Mapping?.Key);
            vm.Name = Caption.GetLocalized();
            vm.Value = Mapping?.Capture(token) ?? string.Empty;
            vm.Type = Editor;
            vm.Enabled = Enabled;
            return vm;
        }
    }
}
