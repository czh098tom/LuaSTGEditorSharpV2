using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable;

public class VariableDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<LocalVariablePropertyViewItemListTerm.ItemTerm>
{
    private readonly BoundProperty _propNameProperty = new();
    private readonly BoundProperty _propValueProperty = new();

    public string PropName
    {
        get => _propNameProperty.Value;
        set => _propNameProperty.Value = value;
    }

    public string PropValue
    {
        get => _propValueProperty.Value;
        set => _propValueProperty.Value = value;
    }

    protected override void ConfigureViewModel(LocalVariablePropertyViewItemListTerm.ItemTerm term)
    {
        ForwardValueChanges(_propNameProperty, nameof(PropName));
        ForwardValueChanges(_propValueProperty, nameof(PropValue));
    }

    protected override void ConfigureBinding(LocalVariablePropertyViewItemListTerm.ItemTerm term)
    {
		Bind(term.NameRule).ToOne(_propNameProperty);
		Bind(term.ValueRule).ToOne(_propValueProperty);
    }
}
