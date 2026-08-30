using System.Windows;
using System.Windows.Controls;

using LuaSTGEditorSharpV2.Core.Editor;

using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class PropertyItemTemplateSelectorTests
{
    [Fact]
    public void EditorTypeSelectsMatchingPropertyTemplate()
    {
        var defaultTemplate = new DataTemplate();
        var numberTemplate = new DataTemplate();
        var resources = new ResourceDictionary
        {
            ["property:number"] = numberTemplate,
        };
        var selector = new TestPropertyItemTemplateSelector(resources)
        {
            Default = defaultTemplate,
        };
        var viewModel = new TestPropertyItemViewModel
        {
            Type = new PropertyViewEditorType("number"),
        };

        var selected = selector.SelectTemplate(viewModel, null!);

        Assert.Same(numberTemplate, selected);
    }

    [Fact]
    public void MissingEditorTypeUsesDefaultTemplate()
    {
        var defaultTemplate = new DataTemplate();
        var selector = new TestPropertyItemTemplateSelector(new ResourceDictionary())
        {
            Default = defaultTemplate,
        };

        var selected = selector.SelectTemplate(new TestPropertyItemViewModel(), null!);

        Assert.Same(defaultTemplate, selected);
    }

    [Fact]
    public void UnknownEditorTypeUsesDefaultTemplate()
    {
        var defaultTemplate = new DataTemplate();
        var selector = new TestPropertyItemTemplateSelector(new ResourceDictionary())
        {
            Default = defaultTemplate,
        };
        var viewModel = new TestPropertyItemViewModel
        {
            Type = new PropertyViewEditorType("missing"),
        };

        var selected = selector.SelectTemplate(viewModel, null!);

        Assert.Same(defaultTemplate, selected);
    }

    private sealed class TestPropertyItemTemplateSelector(ResourceDictionary resources)
        : PropertyItemTemplateSelector
    {
        public override ResourceDictionary GetResourceDictionary()
        {
            return resources;
        }
    }

    private sealed class TestPropertyItemViewModel : PropertyItemViewModelBase
    {
        protected override void HandleEditorNodeOnPropertyChanged(
            object? sender,
            EditorNodePropertyChangedEventArgs e)
        {
        }
    }
}
