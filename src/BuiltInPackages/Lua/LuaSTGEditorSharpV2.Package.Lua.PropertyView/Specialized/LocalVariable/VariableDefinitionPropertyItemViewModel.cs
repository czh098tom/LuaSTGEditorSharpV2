using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable;

public class VariableDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<LocalVariablePropertyViewItemTerm>
{
    private int _index;

    private readonly BoundProperty _propName = new();
    private readonly BoundProperty _propValue = new();

    public string PropName
    {
        get => _propName.Value;
        set => _propName.Value = value;
    }

    public string PropValue
    {
        get => _propValue.Value;
        set => _propValue.Value = value;
    }

    public void Configure(LocalVariablePropertyViewItemTerm term, int index)
    {
        _index = index;
        base.Configure(term);
    }

    protected override void ConfigureViewModel(LocalVariablePropertyViewItemTerm term)
    {
        ForwardValueChanges(_propName, nameof(PropName));
        ForwardValueChanges(_propValue, nameof(PropValue));
    }

    protected override void ConfigureBinding(LocalVariablePropertyViewItemTerm term)
    {
        if (term.NameRule != null)
        {
            Bind(term.NameRule.Format(_index)).ToOne(_propName);
        }
        if (term.ValueRule != null)
        {
            Bind(term.ValueRule.Format(_index)).ToOne(_propValue);
        }
    }
}

[Inject(ServiceLifetime.Singleton)]
public class VariableDefinitionPropertyItemViewModelFactory(
    PropertyEditWizardProviderService propertyEditWizardProviderService)
{
    public VariableDefinitionPropertyItemViewModel Create(
        IReadOnlyList<PropertySource> sources,
        LocalVariablePropertyViewItemTerm term,
        int index,
        PropertyViewEditorType? type,
        LocalServiceParam localServiceParam)
    {
        var viewModel = new VariableDefinitionPropertyItemViewModel();
        viewModel.Initialize(sources, localServiceParam, propertyEditWizardProviderService);
        viewModel.Type = type;
        viewModel.Configure(term, index);
        viewModel.Populate();
        return viewModel;
    }
}
