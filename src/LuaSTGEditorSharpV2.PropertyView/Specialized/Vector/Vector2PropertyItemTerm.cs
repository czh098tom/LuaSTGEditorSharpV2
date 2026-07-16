using System;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Vector;

[JsonUseShortNaming]
[JsonTypeShortName(typeof(IPropertyItemTerm), "Vector2")]
public class Vector2PropertyItemTerm(IServiceProvider serviceProvider) : PropertyItemTerm(serviceProvider)
{
    public override PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context)
    {
        return GetViewModelImpl<Vector2PropertyItemViewModel, Vector2PropertyItemTerm>(nodeData, context, this, Editor);
    }
}
