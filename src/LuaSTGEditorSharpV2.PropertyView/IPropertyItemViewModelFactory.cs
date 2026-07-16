using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView;

public interface IPropertyItemViewModelFactory<out TViewModel, in TTerm>
    where TViewModel: BoundPropertyItemViewModelBase<TTerm>
    where TTerm: PropertyItemTermBase
{
    public TViewModel Create(IReadOnlyList<PropertySource> nodeData, TTerm term,
        PropertyViewEditorType? type, LocalServiceParam localServiceParam);
}
