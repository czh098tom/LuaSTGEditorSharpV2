using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable;

public interface IMultiSourcePropertyItemListTerm : IPropertyItemListTerm
{
    IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
        IReadOnlyList<EditorNode> nodes,
        PropertyViewContext context,
        int count);
}
