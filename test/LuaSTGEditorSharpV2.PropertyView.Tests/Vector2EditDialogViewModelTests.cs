using LuaSTGEditorSharpV2.Dialog.ViewModel;
using LuaSTGEditorSharpV2.Dialog.ViewModel.Vector2EditDialog;
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

        Cartesian(viewModel).Y = "changed";

        Assert.Equal("a+b", viewModel.X);
        Assert.Equal("c+changed", viewModel.Y);
        Assert.Equal("a+b,c+changed", viewModel.Expression);
    }

    [Fact]
    public void SyncCommandsBelongToCartesianState()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("sin(angle)", "cos(angle)");
        viewModel.SelectedComponent = viewModel.Components[0];
        var state = Cartesian(viewModel);

        state.SyncXYCommand.Execute(null);
        Assert.Equal("cos(angle)", viewModel.X);

        state.SyncTrigonometricsCommand.Execute(null);
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
    public void CartesianAddCommandAppendsAndSelectsZeroComponent()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("x", "y");

        viewModel.AddCartesianComponentCommand.Execute(null);

        Assert.Equal(2, viewModel.Components.Count);
        Assert.Equal("(0, 0)", viewModel.Components[1].Display);
        Assert.Same(
            viewModel.Components[1],
            viewModel.SelectedComponent);
        Assert.Equal("x,y", viewModel.Expression);
    }

    [Fact]
    public void PolarAddCommandAppendsEditablePolarPreset()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("x", "y");

        viewModel.AddPolarComponentCommand.Execute(null);

        var component = Selected(viewModel);
        var state = Assert.IsType<PolarEditorState>(
            component.ActiveState);
        Assert.Equal("x+r*cos(a)", viewModel.X);
        Assert.Equal("y+r*sin(a)", viewModel.Y);
        Assert.Equal("r", state.Radius);
        Assert.Equal("a", state.Angle);
    }

    [Fact]
    public void MemberAccessAddCommandAppendsEditableSelfPreset()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("x", "y");

        viewModel.AddMemberAccessComponentCommand.Execute(null);

        var component = Selected(viewModel);
        var state = Assert.IsType<MemberAccessEditorState>(
            component.ActiveState);
        Assert.Equal("x+self.x", viewModel.X);
        Assert.Equal("y+self.y", viewModel.Y);
        Assert.Equal("self", state.Source);
        Assert.Equal("x", state.FirstAccessor);
        Assert.Equal("y", state.SecondAccessor);
    }

    [Fact]
    public void RemoveCommandDeletesSelectedComponentAndSelectsItsNeighbor()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("a+b", "c+d");
        viewModel.SelectedComponent = viewModel.Components[0];

        viewModel.RemoveComponentCommand.Execute(null);

        Assert.Single(viewModel.Components);
        Assert.Equal("(b, d)", viewModel.Components[0].Display);
        Assert.Same(
            viewModel.Components[0],
            viewModel.SelectedComponent);
        Assert.Equal("b,d", viewModel.Expression);
    }

    [Fact]
    public void MoveComponentReordersBothExpressions()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("a+b+c", "x+y+z");
        var moved = viewModel.Components[2];

        viewModel.MoveComponent(moved, 0);

        Assert.Equal("c+a+b", viewModel.X);
        Assert.Equal("z+x+y", viewModel.Y);
        Assert.Same(moved, viewModel.SelectedComponent);
    }

    [Fact]
    public void SelectingPolarExpressionActivatesPolarState()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues(
            "radius * cos(angle)",
            "radius * sin(angle)");
        viewModel.SelectedComponent = viewModel.Components[0];
        var component = Selected(viewModel);

        Assert.False(component.IsCartesianActive);
        Assert.True(component.IsPolarActive);
        Assert.True(component.CanUsePolar);
        Assert.Equal("radius", component.PolarState!.Radius);
        Assert.Equal("angle", component.PolarState.Angle);
    }

    [Fact]
    public void ActivatingPatternPreservesOriginalExpressionFormatting()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues(
            "radius * cos(angle)",
            "radius * sin(angle)");
        viewModel.SelectedComponent = viewModel.Components[0];

        Selected(viewModel).IsPolarActive = true;

        Assert.Equal("radius * cos(angle)", viewModel.X);
        Assert.Equal("radius * sin(angle)", viewModel.Y);
        Assert.True(Selected(viewModel).IsPolarActive);
    }

    [Fact]
    public void PolarEditsRebuildBothComponentsAndAggregateExpression()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("r*cos(a)", "r*sin(a)");
        viewModel.SelectedComponent = viewModel.Components[0];
        Selected(viewModel).IsPolarActive = true;
        var state = Assert.IsType<PolarEditorState>(
            Selected(viewModel).ActiveState);

        state.Radius = "speed+offset";
        state.Angle = "heading";

        Assert.Equal("(speed+offset)*cos(heading)", viewModel.X);
        Assert.Equal("(speed+offset)*sin(heading)", viewModel.Y);
        Assert.Equal(
            "(speed+offset)*cos(heading),(speed+offset)*sin(heading)",
            viewModel.Expression);
        Assert.True(Selected(viewModel).IsPolarActive);
    }

    [Fact]
    public void SpecializedDraftRemainsActiveWhileTemporarilyInvalid()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("r*cos(a)", "r*sin(a)");
        viewModel.SelectedComponent = viewModel.Components[0];
        Selected(viewModel).IsPolarActive = true;
        var state = Assert.IsType<PolarEditorState>(
            Selected(viewModel).ActiveState);

        state.Radius = string.Empty;

        Assert.True(Selected(viewModel).IsPolarActive);
        Assert.Same(state, Selected(viewModel).ActiveState);
        Assert.Equal(string.Empty, viewModel.X);
        Assert.Equal(string.Empty, viewModel.Y);

        Selected(viewModel).IsMemberAccessActive = true;
        state.Radius = "next";

        Assert.Same(state, Selected(viewModel).ActiveState);
        Assert.Equal("next*cos(a)", viewModel.X);
        Assert.Equal("next*sin(a)", viewModel.Y);
    }

    [Fact]
    public void NonPolarComponentCannotSwitchToPolarState()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("x", "y");
        viewModel.SelectedComponent = viewModel.Components[0];

        Selected(viewModel).IsPolarActive = true;

        Assert.False(Selected(viewModel).CanUsePolar);
        Assert.True(Selected(viewModel).IsCartesianActive);
    }

    [Fact]
    public void CartesianEditThatBreaksPatternRemovesPolarCandidate()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("r*cos(a)", "r*sin(a)");
        viewModel.SelectedComponent = viewModel.Components[0];
        Selected(viewModel).IsCartesianActive = true;

        Cartesian(viewModel).X = "x";

        Assert.Equal("x", viewModel.X);
        Assert.False(Selected(viewModel).CanUsePolar);
        Assert.True(Selected(viewModel).IsCartesianActive);
    }

    [Fact]
    public void ActiveStateBelongsToEachSubVector()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues(
            "r*cos(a)+x",
            "r*sin(a)+y");

        viewModel.SelectedComponent = viewModel.Components[0];
        Selected(viewModel).IsPolarActive = true;
        viewModel.SelectedComponent = viewModel.Components[1];

        Assert.False(Selected(viewModel).CanUsePolar);
        Assert.True(Selected(viewModel).IsCartesianActive);

        viewModel.SelectedComponent = viewModel.Components[0];
        Assert.True(Selected(viewModel).IsPolarActive);
    }

    [Fact]
    public void ReselectingComponentPreservesExplicitMode()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues(
            "r*cos(a)+x",
            "r*sin(a)+y");
        var polarComponent = viewModel.Components[0];
        var cartesianComponent = viewModel.Components[1];

        viewModel.SelectedComponent = polarComponent;
        polarComponent.IsCartesianActive = true;
        viewModel.SelectedComponent = cartesianComponent;
        viewModel.SelectedComponent = polarComponent;

        Assert.True(polarComponent.IsCartesianActive);
        Assert.IsType<CartesianEditorState>(
            polarComponent.ActiveState);
    }

    [Fact]
    public void MemberAccessCandidateUsesDedicatedState()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues(
            "factory()[key].x",
            "factory()[key].y");
        viewModel.SelectedComponent = viewModel.Components[0];

        Selected(viewModel).IsMemberAccessActive = true;
        var state = Assert.IsType<MemberAccessEditorState>(
            Selected(viewModel).ActiveState);
        state.Source = "other";

        Assert.Equal("other.x", viewModel.X);
        Assert.Equal("other.y", viewModel.Y);
        Assert.True(Selected(viewModel).IsMemberAccessActive);
    }

    [Fact]
    public void IndexAccessCandidateUsesDedicatedState()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("value[1]", "value[2]");
        viewModel.SelectedComponent = viewModel.Components[0];

        Selected(viewModel).IsIndexAccessActive = true;
        var state = Assert.IsType<IndexAccessEditorState>(
            Selected(viewModel).ActiveState);
        state.SecondAccessor = "index";

        Assert.Equal("value[1]", viewModel.X);
        Assert.Equal("value[index]", viewModel.Y);
        Assert.True(Selected(viewModel).IsIndexAccessActive);
    }

    [Fact]
    public void ExternalExpressionChangeRebuildsAndActivatesLastMatchedState()
    {
        var viewModel = new Vector2EditDialogViewModel();
        viewModel.SetInitialValues("r*cos(a)", "r*sin(a)");
        viewModel.SelectedComponent = viewModel.Components[0];
        Selected(viewModel).IsPolarActive = true;

        viewModel.Expression = "next.x,next.y";

        Assert.Single(viewModel.Components);
        Assert.False(viewModel.Components[0].IsCartesianActive);
        Assert.True(viewModel.Components[0].IsMemberAccessActive);
        Assert.True(viewModel.Components[0].CanUseMemberAccess);
    }

    [Fact]
    public void NoSelectionDisablesModeEditingState()
    {
        var viewModel = new Vector2EditDialogViewModel();

        Assert.False(viewModel.HasSelectedComponent);

        viewModel.SelectedComponent = null;

        Assert.False(viewModel.HasSelectedComponent);
    }

    private static Vector2ComponentViewModel Selected(
        Vector2EditDialogViewModel viewModel)
        => Assert.IsType<Vector2ComponentViewModel>(
            viewModel.SelectedComponent);

    private static CartesianEditorState Cartesian(
        Vector2EditDialogViewModel viewModel)
        => Assert.IsType<CartesianEditorState>(
            Selected(viewModel).ActiveState);
}
