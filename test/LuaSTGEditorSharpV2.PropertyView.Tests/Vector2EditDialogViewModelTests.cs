using LuaSTGEditorSharpV2.Dialog.ViewModel;
using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class Vector2EditDialogViewModelTests
{
    [Fact]
    public void ExpressionBuildsPairedPolynomialComponents()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("a+b", "c");

        Assert.Collection(
            viewModel.Components,
            first => Assert.Equal("(a, c)", first.Display),
            second => Assert.Equal("(b, 0)", second.Display));
    }

    [Fact]
    public void SelectedComponentEditMergesBackIntoExpression()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("a+b", "c+d");
        viewModel.SelectedComponent = viewModel.Components[1];

        viewModel.CurrentY = "changed";

        Assert.Equal("a+b", viewModel.X);
        Assert.Equal("c+changed", viewModel.Y);
        Assert.Equal("a+b,c+changed", viewModel.Expression);
    }

    [Fact]
    public void SyncCommandsFollowTheSelectedComponent()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("sin(angle)", "cos(angle)");
        viewModel.SelectedComponent = viewModel.Components[0];

        viewModel.SyncXYCommand.Execute(null);
        Assert.Equal("cos(angle)", viewModel.X);

        viewModel.SyncTrigonometricsCommand.Execute(null);
        Assert.Equal("sin(angle)", viewModel.Y);
    }

    [Fact]
    public void AppendingVectorAddsBothComponentsAsTerms()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("x", "y");

        viewModel.AppendVector(10, -2.5);

        Assert.Equal("x+10", viewModel.X);
        Assert.Equal("y-2.5", viewModel.Y);
    }

    [Fact]
    public void PolarPatternIsAvailableWithoutChangingSourceExpressions()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("radius * cos(angle)", "radius * sin(angle)");
        viewModel.SelectedComponent = viewModel.Components[0];

        Assert.True(viewModel.CanCurrentUsePolar);
        Assert.Equal("radius", viewModel.CurrentRadius);
        Assert.Equal("angle", viewModel.CurrentAngle);

        viewModel.IsCurrentPolar = true;

        Assert.True(viewModel.IsCurrentPolar);
        Assert.Equal("radius * cos(angle)", viewModel.X);
        Assert.Equal("radius * sin(angle)", viewModel.Y);
    }

    [Fact]
    public void PolarEditsRebuildBothComponentsAndAggregateExpression()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("r*cos(a)", "r*sin(a)");
        viewModel.SelectedComponent = viewModel.Components[0];
        viewModel.IsCurrentPolar = true;

        viewModel.CurrentRadius = "speed+offset";
        viewModel.CurrentAngle = "heading";

        Assert.Equal("(speed+offset)*cos(heading)", viewModel.X);
        Assert.Equal("(speed+offset)*sin(heading)", viewModel.Y);
        Assert.Equal("(speed+offset)*cos(heading),(speed+offset)*sin(heading)", viewModel.Expression);
        Assert.True(viewModel.IsCurrentPolar);
        Assert.Same(viewModel.Components[0], viewModel.SelectedComponent);
    }

    [Fact]
    public void NonPolarComponentCannotSwitchToPolarMode()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("x", "y");
        viewModel.SelectedComponent = viewModel.Components[0];

        viewModel.IsCurrentPolar = true;

        Assert.False(viewModel.CanCurrentUsePolar);
        Assert.True(viewModel.IsCurrentCartesian);
        Assert.False(viewModel.IsCurrentPolar);
    }

    [Fact]
    public void CartesianEditThatBreaksPatternDisablesPolarMode()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("r*cos(a)", "r*sin(a)");
        viewModel.SelectedComponent = viewModel.Components[0];
        viewModel.IsCurrentPolar = true;

        viewModel.CurrentX = "x";

        Assert.Equal("x", viewModel.X);
        Assert.False(viewModel.CanCurrentUsePolar);
        Assert.True(viewModel.IsCurrentCartesian);
    }

    [Fact]
    public void PolarModeBelongsToEachSubVector()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("r*cos(a)+x", "r*sin(a)+y");

        viewModel.SelectedComponent = viewModel.Components[0];
        viewModel.IsCurrentPolar = true;
        viewModel.SelectedComponent = viewModel.Components[1];

        Assert.False(viewModel.CanCurrentUsePolar);
        Assert.True(viewModel.IsCurrentCartesian);

        viewModel.SelectedComponent = viewModel.Components[0];
        Assert.True(viewModel.IsCurrentPolar);
    }
}
