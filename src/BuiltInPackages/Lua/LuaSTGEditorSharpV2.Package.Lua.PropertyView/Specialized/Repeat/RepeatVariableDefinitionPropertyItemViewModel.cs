using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat;

public class RepeatVariableDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<RepeatPropertyViewItemListTerm.ItemTerm>
{
    private readonly BoundProperty _propNameProperty = new();
    private readonly BoundProperty _propInitProperty = new();
    private readonly BoundProperty _propIncrementProperty = new();

    public string PropName
    {
        get => _propNameProperty.Value;
        set => _propNameProperty.Value = value;
    }

    public string PropInit
    {
        get => _propInitProperty.Value;
        set => _propInitProperty.Value = value;
    }

    public string PropIncrement
    {
        get => _propIncrementProperty.Value;
        set => _propIncrementProperty.Value = value;
    }

    protected override void ConfigureViewModel(RepeatPropertyViewItemListTerm.ItemTerm term)
    {
        ForwardValueChanges(_propNameProperty, nameof(PropName));
        ForwardValueChanges(_propInitProperty, nameof(PropInit));
        ForwardValueChanges(_propIncrementProperty, nameof(PropIncrement));
    }

    protected override void ConfigureBinding(RepeatPropertyViewItemListTerm.ItemTerm term)
	{
		Bind(term.Name).ToOne(_propNameProperty);
		Bind(term.Initial).ToOne(_propInitProperty);
		Bind(term.Increment).ToOne(_propIncrementProperty);
	}
}