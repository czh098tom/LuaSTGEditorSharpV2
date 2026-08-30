using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.PropertyView.ViewModel;

using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class PropertyEditWizardRegistererTests
{
    [Fact]
    public void CodeWizardAppliesAcceptedValue()
    {
        var dialogService = new TestCodeEditDialogService("edited");
        using var services = CreateServices(dialogService);
        var wizard = GetWizard(services, "code");
        var viewModel = new BasicPropertyItemViewModel
        {
            Name = "Code",
            Value = "initial",
        };

        var result = wizard.EditValue(viewModel, null!);

        Assert.Null(result);
        Assert.Equal("Code", dialogService.RequestedTitle);
        Assert.Equal("initial", dialogService.InitialValue);
        Assert.Equal("edited", viewModel.Value);
    }

    [Fact]
    public void CodeWizardKeepsValueWhenCanceled()
    {
        var dialogService = new TestCodeEditDialogService(null);
        using var services = CreateServices(dialogService);
        var wizard = GetWizard(services, "code");
        var viewModel = new BasicPropertyItemViewModel
        {
            Name = "Code",
            Value = "initial",
        };

        var result = wizard.EditValue(viewModel, null!);

        Assert.Null(result);
        Assert.Equal("initial", viewModel.Value);
    }

    [Fact]
    public void MultilineTextWizardAppliesAcceptedValue()
    {
        var dialogService = new TestMultilineTextEditDialogService("first\nsecond");
        using var services = new ServiceCollection()
            .AddSingleton<IMultilineTextEditDialogService>(dialogService)
            .BuildServiceProvider();
        var wizard = GetWizard(services, "multilineText");
        var viewModel = new BasicPropertyItemViewModel
        {
            Name = "Text",
            Value = "initial",
        };

        var result = wizard.EditValue(viewModel, null!);

        Assert.Null(result);
        Assert.Equal("Text", dialogService.RequestedTitle);
        Assert.Equal("initial", dialogService.InitialValue);
        Assert.Equal("first\nsecond", viewModel.Value);
    }

    private static ServiceProvider CreateServices(ICodeEditDialogService dialogService)
        => new ServiceCollection()
            .AddSingleton(dialogService)
            .BuildServiceProvider();

    private static PropertyEditWizardBase GetWizard(IServiceProvider services, string name)
        => Assert.Single(
            new PropertyEditWizardRegisterer().GetServiceInstances(services),
            wizard => wizard.Name == name);
}
