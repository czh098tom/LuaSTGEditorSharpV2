using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.PropertyView.Specialized.SmoothSetValueTo;

public class SmoothSetValueDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<SmoothSetValuePropertyViewItemTerm>
{
    private int _index;

    private readonly BoundProperty _variableName = new();
    private readonly BoundProperty _targetValue = new();
    private readonly BoundProperty _interpolationMode = new();
    private readonly BoundProperty _modificationMode = new();

    public string VariableName
    {
        get => _variableName.Value;
        set => _variableName.Value = value;
    }

    public string TargetValue
    {
        get => _targetValue.Value;
        set => _targetValue.Value = value;
    }

    public string InterpolationMode
    {
        get => _interpolationMode.Value;
        set => _interpolationMode.Value = value;
    }

    public string ModificationMode
    {
        get => _modificationMode.Value;
        set => _modificationMode.Value = value;
    }

    public void Configure(SmoothSetValuePropertyViewItemTerm term, int index)
    {
        _index = index;
        base.Configure(term);
    }

    protected override void ConfigureViewModel(SmoothSetValuePropertyViewItemTerm term)
    {
        ForwardValueChanges(_variableName, nameof(VariableName));
        ForwardValueChanges(_targetValue, nameof(TargetValue));
        ForwardValueChanges(_interpolationMode, nameof(InterpolationMode));
        ForwardValueChanges(_modificationMode, nameof(ModificationMode));
    }

    protected override void ConfigureBinding(SmoothSetValuePropertyViewItemTerm term)
    {
        if (term.VariableNameRule != null)
        {
            Bind(term.VariableNameRule.Format(_index)).ToOne(_variableName);
        }
        if (term.TargetValueRule != null)
        {
            Bind(term.TargetValueRule.Format(_index)).ToOne(_targetValue);
        }
        if (term.InterpolationModeRule != null)
        {
            Bind(term.InterpolationModeRule.Format(_index)).ToOne(_interpolationMode);
        }
        if (term.ModificationModeRule != null)
        {
            Bind(term.ModificationModeRule.Format(_index)).ToOne(_modificationMode);
        }
    }
}

[Inject(ServiceLifetime.Singleton)]
public class SmoothSetValueDefinitionPropertyItemViewModelFactory(
    PropertyEditWizardProviderService propertyEditWizardProviderService)
{
    public SmoothSetValueDefinitionPropertyItemViewModel Create(
        IReadOnlyList<PropertySource> sources,
        SmoothSetValuePropertyViewItemTerm term,
        int index,
        PropertyViewEditorType? type,
        LocalServiceParam localServiceParam)
    {
        var viewModel = new SmoothSetValueDefinitionPropertyItemViewModel();
        viewModel.Initialize(sources, localServiceParam, propertyEditWizardProviderService);
        viewModel.Type = type;
        viewModel.Configure(term, index);
        viewModel.Populate();
        return viewModel;
    }
}
