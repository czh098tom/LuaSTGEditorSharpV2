using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.PropertyView.Specialized.LinqSTGBlueprintPatternButton
{
    [Inject(ServiceLifetime.Transient)]
    [JsonTypeShortName(typeof(IPropertyItemTerm), "LinqSTGBlueprintPatternButton")]
    public class LinqSTGBlueprintPatternButtonItemTerm(IServiceProvider serviceProvider)
        : PropertyItemTerm(serviceProvider)
    {
        public override PropertyItemViewModelBase GetViewModel(
            IReadOnlyList<EditorNode> nodes,
            PropertyViewContext context)
        {
            var factory = ServiceProvider.GetRequiredService<
                IPropertyItemViewModelFactory<LinqSTGBlueprintPatternButtonViewModel,
                    LinqSTGBlueprintPatternButtonItemTerm>>();
            return factory.Create(nodes, this, Editor, context);
        }
    }
}
