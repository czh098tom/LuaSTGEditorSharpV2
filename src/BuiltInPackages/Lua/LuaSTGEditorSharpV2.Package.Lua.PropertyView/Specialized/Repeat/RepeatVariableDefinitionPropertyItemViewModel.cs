using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat;

public class RepeatVariableDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<RepeatPropertyViewItemListTerm.ItemTerm>
{
    public BoundProperty PropNameProperty { get; } = new();
    public BoundProperty PropInitProperty { get; } = new();
    public BoundProperty PropIncrementProperty { get; } = new();

    public string PropName
    {
        get => PropNameProperty.Value;
        set => PropNameProperty.Value = value;
    }

    public string PropInit
    {
        get => PropInitProperty.Value;
        set => PropInitProperty.Value = value;
    }

    public string PropIncrement
    {
        get => PropIncrementProperty.Value;
        set => PropIncrementProperty.Value = value;
    }

    protected override void ConfigureViewModel(RepeatPropertyViewItemListTerm.ItemTerm term)
    {
        ForwardValueChanges(PropNameProperty, nameof(PropName));
        ForwardValueChanges(PropInitProperty, nameof(PropInit));
        ForwardValueChanges(PropIncrementProperty, nameof(PropIncrement));
    }

    protected override void ConfigureBinding(RepeatPropertyViewItemListTerm.ItemTerm term)
	{
		Bind(term.Name).ToOne(PropNameProperty);
		Bind(term.Initial).ToOne(PropInitProperty);
		Bind(term.Increment).ToOne(PropIncrementProperty);
	}
}
