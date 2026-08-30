using System.Collections.Generic;

using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView;

public interface IMultiSourcePropertyItemTerm : IPropertyItemTerm
{
    PropertyItemViewModelBase GetViewModel(
        IReadOnlyList<EditorNode> nodeData,
        PropertyViewContext context);
}
