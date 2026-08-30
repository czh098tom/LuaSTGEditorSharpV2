using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable;

public interface IPropertyItemListTerm
{
    IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
        EditorNode nodeData,
        PropertyViewContext context,
        int count);
}
