using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.PropertyView.Specialized.SmoothSetValueTo;

public class SmoothSetValueDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<SmoothSetValuePropertyViewItemListTerm.ItemTerm>
{
    public BoundProperty VariableNameProperty { get; } = new();
    public BoundProperty TargetValueProperty { get; } = new();
    public BoundProperty InterpolationModeProperty { get; } = new();
    public BoundProperty ModificationModeProperty { get; } = new();

    public string VariableName
    {
        get => VariableNameProperty.Value;
        set => VariableNameProperty.Value = value;
    }

    public string TargetValue
    {
        get => TargetValueProperty.Value;
        set => TargetValueProperty.Value = value;
    }

    public string InterpolationMode
    {
        get => InterpolationModeProperty.Value;
        set => InterpolationModeProperty.Value = value;
    }

    public string ModificationMode
    {
        get => ModificationModeProperty.Value;
        set => ModificationModeProperty.Value = value;
    }

    protected override void ConfigureViewModel(SmoothSetValuePropertyViewItemListTerm.ItemTerm term)
    {
        ForwardValueChanges(VariableNameProperty, nameof(VariableName));
        ForwardValueChanges(TargetValueProperty, nameof(TargetValue));
        ForwardValueChanges(InterpolationModeProperty, nameof(InterpolationMode));
        ForwardValueChanges(ModificationModeProperty, nameof(ModificationMode));
    }

    protected override void ConfigureBinding(SmoothSetValuePropertyViewItemListTerm.ItemTerm term)
	{
		Bind(term.VariableName).ToOne(VariableNameProperty);
		Bind(term.TargetValue).ToOne(TargetValueProperty);
		Bind(term.InterpolationMode).ToOne(InterpolationModeProperty);
		Bind(term.ModificationMode).ToOne(ModificationModeProperty);
	}
}
