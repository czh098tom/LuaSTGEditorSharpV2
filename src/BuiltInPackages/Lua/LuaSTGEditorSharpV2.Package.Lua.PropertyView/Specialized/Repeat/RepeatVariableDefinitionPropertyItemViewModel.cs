using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat;

public class RepeatVariableDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<RepeatPropertyViewItemTerm>
{
    private int _index;

    private readonly BoundProperty _propName = new();
    private readonly BoundProperty _propInit = new();
    private readonly BoundProperty _propIncrement = new();

    public string PropName
    {
        get => _propName.Value;
        set => _propName.Value = value;
    }

    public string PropInit
    {
        get => _propInit.Value;
        set => _propInit.Value = value;
    }

    public string PropIncrement
    {
        get => _propIncrement.Value;
        set => _propIncrement.Value = value;
    }

    public void Configure(RepeatPropertyViewItemTerm term, int index)
    {
        _index = index;
        base.Configure(term);
    }

    protected override void ConfigureViewModel(RepeatPropertyViewItemTerm term)
    {
        ForwardValueChanges(_propName, nameof(PropName));
        ForwardValueChanges(_propInit, nameof(PropInit));
        ForwardValueChanges(_propIncrement, nameof(PropIncrement));
    }

    protected override void ConfigureBinding(RepeatPropertyViewItemTerm term)
    {
        if (term.NameRule != null)
        {
            Bind(term.NameRule.Format(_index)).ToOne(_propName);
        }
        if (term.InitRule != null)
        {
            Bind(term.InitRule.Format(_index)).ToOne(_propInit);
        }
        if (term.IncrementRule != null)
        {
            Bind(term.IncrementRule.Format(_index)).ToOne(_propIncrement);
        }
    }
}

[Inject(ServiceLifetime.Singleton)]
public class RepeatVariableDefinitionPropertyItemViewModelFactory(
    PropertyEditWizardProviderService propertyEditWizardProviderService)
{
    public RepeatVariableDefinitionPropertyItemViewModel Create(
        IReadOnlyList<PropertySource> sources,
        RepeatPropertyViewItemTerm term,
        int index,
        PropertyViewEditorType? type,
        LocalServiceParam localServiceParam)
    {
        var viewModel = new RepeatVariableDefinitionPropertyItemViewModel();
        viewModel.Initialize(sources, localServiceParam, propertyEditWizardProviderService);
        viewModel.Type = type;
        viewModel.Configure(term, index);
        viewModel.Populate();
        return viewModel;
    }
}
