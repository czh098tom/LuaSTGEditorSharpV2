using System.Collections.Generic;

using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable;

public interface IMultiSourcePropertyTabTerm
{
    PropertyTabViewModel GetPropertyTabViewModel(
        IReadOnlyList<EditorNode> nodeData,
        PropertyViewContext context);
}
