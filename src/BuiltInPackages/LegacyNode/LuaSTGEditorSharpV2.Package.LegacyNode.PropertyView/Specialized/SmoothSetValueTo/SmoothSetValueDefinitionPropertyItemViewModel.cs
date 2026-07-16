using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.PropertyView.Specialized.SmoothSetValueTo;

public class SmoothSetValueDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<SmoothSetValuePropertyViewItemListTerm.ItemTerm>
{
    private readonly BoundProperty _variableNameProperty = new();
    private readonly BoundProperty _targetValueProperty = new();
    private readonly BoundProperty _interpolationModeProperty = new();
    private readonly BoundProperty _modificationModeProperty = new();

    public string VariableName
    {
        get => _variableNameProperty.Value;
        set => _variableNameProperty.Value = value;
    }

    public string TargetValue
    {
        get => _targetValueProperty.Value;
        set => _targetValueProperty.Value = value;
    }

    public string InterpolationMode
    {
        get => _interpolationModeProperty.Value;
        set => _interpolationModeProperty.Value = value;
    }

    public string ModificationMode
    {
        get => _modificationModeProperty.Value;
        set => _modificationModeProperty.Value = value;
    }

    protected override void ConfigureViewModel(SmoothSetValuePropertyViewItemListTerm.ItemTerm term)
    {
        ForwardValueChanges(_variableNameProperty, nameof(VariableName));
        ForwardValueChanges(_targetValueProperty, nameof(TargetValue));
        ForwardValueChanges(_interpolationModeProperty, nameof(InterpolationMode));
        ForwardValueChanges(_modificationModeProperty, nameof(ModificationMode));
    }

    protected override void ConfigureBinding(SmoothSetValuePropertyViewItemListTerm.ItemTerm term)
	{
		Bind(term.VariableName).ToOne(_variableNameProperty);
		Bind(term.TargetValue).ToOne(_targetValueProperty);
		Bind(term.InterpolationMode).ToOne(_interpolationModeProperty);
		Bind(term.ModificationMode).ToOne(_modificationModeProperty);
	}
}