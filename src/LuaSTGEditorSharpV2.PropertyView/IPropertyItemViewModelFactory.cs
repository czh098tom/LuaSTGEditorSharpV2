using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView;

public interface IPropertyItemViewModelFactory<out TViewModel, in TTerm>
    where TTerm : PropertyItemTermBase
    where TViewModel : BoundPropertyItemViewModelBase<TTerm>
{
    TViewModel Create(
        IReadOnlyList<EditorNode> nodes,
        TTerm term,
        PropertyViewEditorType? type,
        PropertyViewContext context);
}
