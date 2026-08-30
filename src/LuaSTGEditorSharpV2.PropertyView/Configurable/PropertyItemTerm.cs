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
    public class PropertyItemTerm(IServiceProvider serviceProvider)
        : PropertyItemTermBase(serviceProvider)
    {
        private string? _captionOverride;

        [JsonProperty(Required = Required.Always)] public NodePropertyCapture Mapping { get; private set; } = null!;
        [JsonProperty] public LocalizableString Caption { get; private set; } = new();

        [JsonIgnore]
        public string ResolvedCaption => _captionOverride ?? Caption.GetLocalized();

        internal static PropertyItemTerm CreateNative(IServiceProvider serviceProvider, string key)
        {
            return new PropertyItemTerm(serviceProvider)
            {
                Mapping = NodePropertyCapture.FromKey(key),
                _captionOverride = key,
            };
        }

        public override PropertyItemViewModelBase GetViewModel(IReadOnlyList<EditorNode> nodes, PropertyViewContext context)
        {
            var factory = ServiceProvider.GetRequiredService<
                IPropertyItemViewModelFactory<BasicPropertyItemViewModel, PropertyItemTerm>>();
            return factory.Create(nodes, this, Editor, context);
        }
    }
}
