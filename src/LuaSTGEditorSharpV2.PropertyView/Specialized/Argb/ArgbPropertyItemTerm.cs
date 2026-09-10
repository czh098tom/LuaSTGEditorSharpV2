using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Argb;

[JsonUseShortNaming]
[JsonTypeShortName(typeof(IPropertyItemTerm), "Argb")]
public class ArgbPropertyItemTerm(IServiceProvider serviceProvider)
    : PropertyItemTerm(serviceProvider)
{
    public override PropertyItemViewModelBase GetViewModel(
        IReadOnlyList<EditorNode> nodes,
        PropertyViewContext context)
    {
        var factory = ServiceProvider.GetRequiredService<
            IPropertyItemViewModelFactory<
                ArgbPropertyItemViewModel,
                ArgbPropertyItemTerm>>();
        return factory.Create(nodes, this, Editor, context);
    }
}
