using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable;

public class VariableDefinitionPropertyItemViewModel
    : BoundPropertyItemViewModelBase<LocalVariablePropertyViewItemListTerm.ItemTerm>
{
    public BoundProperty PropNameProperty { get; } = new();
    public BoundProperty PropValueProperty { get; } = new();

    public string PropName
    {
        get => PropNameProperty.Value;
        set => PropNameProperty.Value = value;
    }

    public string PropValue
    {
        get => PropValueProperty.Value;
        set => PropValueProperty.Value = value;
    }

    protected override void ConfigureViewModel(LocalVariablePropertyViewItemListTerm.ItemTerm term)
    {
        ForwardValueChanges(PropNameProperty, nameof(PropName));
        ForwardValueChanges(PropValueProperty, nameof(PropValue));
    }

    protected override void ConfigureBinding(LocalVariablePropertyViewItemListTerm.ItemTerm term)
    {
		Bind(term.NameRule).ToOne(PropNameProperty);
		Bind(term.ValueRule).ToOne(PropValueProperty);
    }
}
