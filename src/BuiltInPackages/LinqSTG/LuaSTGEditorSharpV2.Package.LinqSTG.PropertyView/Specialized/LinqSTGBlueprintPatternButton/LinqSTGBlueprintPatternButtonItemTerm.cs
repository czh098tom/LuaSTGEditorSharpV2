using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.PropertyView.Specialized.LinqSTGBlueprintPatternButton
{
    [Inject(ServiceLifetime.Transient)]
    [JsonTypeShortName(typeof(IPropertyItemTerm), "LinqSTGBlueprintPatternButton")]
    public class LinqSTGBlueprintPatternButtonItemTerm(IServiceProvider serviceProvider)
        : IPropertyItemTerm
    {
        [JsonProperty] public NodePropertyCapture? Mapping { get; private set; }
        [JsonProperty] public LocalizableString Caption { get; private set; } = new();
        [JsonProperty] public PropertyViewEditorType? Editor { get; private set; }

        public PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData.Source, context);
            var vm = serviceProvider.GetRequiredService<IBasicPropertyItemViewModelFactory<LinqSTGBlueprintPatternButtonViewModel>>()
                .Create([nodeData], Mapping?.Key, BatchEditStatus.AllSame, context.LocalParam);
            vm.ButtonCaption = Caption.GetLocalized();
            vm.Value = Mapping?.Capture(token) ?? string.Empty;
            vm.Type = Editor;
            return vm;
        }
    }
}
