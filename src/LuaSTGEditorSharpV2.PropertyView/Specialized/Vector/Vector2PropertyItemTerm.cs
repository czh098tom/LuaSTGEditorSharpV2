using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Vector;

[JsonUseShortNaming]
[JsonTypeShortName(typeof(IPropertyItemTerm), "Vector2")]
public class Vector2PropertyItemTerm(IServiceProvider serviceProvider) : PropertyItemTerm(serviceProvider)
{
    public override PropertyItemViewModelBase GetViewModel(
        IReadOnlyList<EditorNode> nodes,
        PropertyViewContext context)
    {
        var factory = ServiceProvider.GetRequiredService<
            IPropertyItemViewModelFactory<Vector2PropertyItemViewModel, Vector2PropertyItemTerm>>();
        return factory.Create(nodes, this, Editor, context);
    }
}
