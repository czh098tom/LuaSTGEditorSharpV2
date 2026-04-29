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
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    [JsonUseShortNaming]
    [JsonTypeShortName(typeof(IPropertyItemTerm), "Default")]
    public class PropertyItemTerm(IServiceProvider serviceProvider): IPropertyItemTerm
    {
		[JsonProperty] public NodePropertyCapture? Mapping { get; private set; }
        [JsonProperty] public LocalizableString Caption { get; private set; } = new();
        [JsonProperty] public PropertyViewEditorType? Editor { get; protected set; }
        [JsonProperty] public bool Enabled { get; private set; } = true;

        public virtual PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            return GetViewModelImpl<BasicPropertyItemViewModel>(nodeData, context);
        }
        
        protected PropertyItemViewModelBase GetViewModelImpl<TResult>(EditorNode nodeData, PropertyViewContext context)
            where TResult : BasicPropertyItemViewModel
            => GetViewModelImpl<TResult, IBasicPropertyItemViewModelFactory<TResult>>(nodeData, context);

        protected PropertyItemViewModelBase GetViewModelImpl<TResult, TFactory>(EditorNode nodeData, PropertyViewContext context)
            where TResult : BasicPropertyItemViewModel
            where TFactory : IBasicPropertyItemViewModelFactory<TResult>
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData.Source, context);
            var factory = serviceProvider.GetRequiredService<TFactory>();
            var vm = factory
                .Create([nodeData], Mapping?.Key, BatchEditStatus.AllSame, context.LocalParam);
            vm.Name = Caption.GetLocalized();
            vm.Value = Mapping?.Capture(token) ?? string.Empty;
            vm.Type = Editor;
            vm.Enabled = Enabled;
            return vm;
        }
    }
}
