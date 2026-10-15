using LuaSTGEditorSharpV2.Core.Parsing.Vector2;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Dialog.ViewModel.Vector2EditDialog;

public sealed class Vector2ComponentViewModel : ViewModelBase
{
    private readonly CartesianEditorState _cartesianState;
    private Vector2ComponentEditorState _activeState;
    private Vector2ExpressionPair _expression;
    private PolarEditorState? _polarState;
    private MemberAccessEditorState? _memberAccessState;
    private IndexAccessEditorState? _indexAccessState;

    internal event EventHandler? ExpressionChanged;

    public Vector2ComponentEditorState ActiveState => _activeState;

    public CartesianEditorState CartesianState => _cartesianState;

    public PolarEditorState? PolarState => _polarState;

    public MemberAccessEditorState? MemberAccessState
        => _memberAccessState;

    public IndexAccessEditorState? IndexAccessState
        => _indexAccessState;

    public bool CanUsePolar => PolarState is not null;

    public bool CanUseMemberAccess => MemberAccessState is not null;

    public bool CanUseIndexAccess => IndexAccessState is not null;

    public bool IsCartesianActive
    {
        get => ReferenceEquals(ActiveState, CartesianState);
        set
        {
            if (value)
            {
                ActivateCartesian();
            }
        }
    }

    public bool IsPolarActive
    {
        get => ReferenceEquals(ActiveState, PolarState);
        set
        {
            if (value)
            {
                ActivatePolar();
            }
        }
    }

    public bool IsMemberAccessActive
    {
        get => ReferenceEquals(ActiveState, MemberAccessState);
        set
        {
            if (value)
            {
                ActivateMemberAccess();
            }
        }
    }

    public bool IsIndexAccessActive
    {
        get => ReferenceEquals(ActiveState, IndexAccessState);
        set
        {
            if (value)
            {
                ActivateIndexAccess();
            }
        }
    }

    public Vector2ExpressionPair Expression => _expression;

    public string Display => $"({Expression.X}, {Expression.Y})";

    internal static Vector2ComponentViewModel Create(
        Vector2ComponentForm form)
    {
        var component = new Vector2ComponentViewModel(
            new CartesianEditorState(
                form.Cartesian.X,
                form.Cartesian.Y),
            Vector2ComponentEditorStateFactory.CreatePolarState(form.Polar),
            Vector2ComponentEditorStateFactory.CreateMemberAccessState(
                form.MemberAccess),
            Vector2ComponentEditorStateFactory.CreateIndexAccessState(
                form.IndexAccess));
        component.ActivatePreferredMode();
        return component;
    }

    internal static Vector2ComponentViewModel CreateCartesian(
        string x,
        string y)
        => Create(new Vector2ComponentForm(
            new Vector2ExpressionPair(x, y),
            null,
            null,
            null));

    internal static Vector2ComponentViewModel CreatePolar(
        string radius,
        string angle)
    {
        var state = new PolarEditorState(radius, angle);
        var component = Create(
            Vector2ExpressionCodec.DecodeComponent(state.Expression));
        component.IsPolarActive = true;
        return component;
    }

    internal static Vector2ComponentViewModel CreateMemberAccess(
        string source,
        string firstAccessor,
        string secondAccessor)
    {
        var state = new MemberAccessEditorState(
            source,
            firstAccessor,
            secondAccessor);
        var component = Create(
            Vector2ExpressionCodec.DecodeComponent(state.Expression));
        component.IsMemberAccessActive = true;
        return component;
    }

    internal Vector2ComponentViewModel(
        CartesianEditorState cartesianState,
        PolarEditorState? polarState,
        MemberAccessEditorState? memberAccessState,
        IndexAccessEditorState? indexAccessState)
    {
        _cartesianState = cartesianState;
        _activeState = cartesianState;
        _expression = cartesianState.Expression;
        Subscribe(cartesianState);
        SetPolarState(polarState);
        SetMemberAccessState(memberAccessState);
        SetIndexAccessState(indexAccessState);
    }

    internal void ActivatePreferredMode()
    {
        if (IndexAccessState is not null)
        {
            SetActiveState(IndexAccessState);
            return;
        }

        if (MemberAccessState is not null)
        {
            SetActiveState(MemberAccessState);
            return;
        }

        if (PolarState is not null)
        {
            SetActiveState(PolarState);
            return;
        }

        SetActiveState(CartesianState);
    }

    private void ActivateCartesian()
    {
        if (IsCartesianActive)
        {
            return;
        }

        SynchronizeCandidatesFromActiveState();
        SetActiveState(CartesianState);
    }

    private void ActivatePolar()
    {
        if (IsPolarActive)
        {
            return;
        }

        if (PolarState is not null)
        {
            SetActiveState(PolarState);
        }
    }

    private void ActivateMemberAccess()
    {
        if (IsMemberAccessActive)
        {
            return;
        }

        if (MemberAccessState is not null)
        {
            SetActiveState(MemberAccessState);
        }
    }

    private void ActivateIndexAccess()
    {
        if (IsIndexAccessActive)
        {
            return;
        }

        if (IndexAccessState is not null)
        {
            SetActiveState(IndexAccessState);
        }
    }

    private void SynchronizeCandidatesFromActiveState()
    {
        if (IsCartesianActive)
        {
            return;
        }

        CartesianState.SetExpression(Expression);
        RefreshCandidates();
    }

    private void SetActiveState(Vector2ComponentEditorState state)
    {
        if (ReferenceEquals(_activeState, state))
        {
            return;
        }

        _activeState = state;
        RaisePropertyChanged(nameof(ActiveState));
        RaiseModePropertiesChanged();
    }

    private void OnEditorExpressionChanged(object? sender, EventArgs e)
    {
        if (!ReferenceEquals(sender, ActiveState))
        {
            return;
        }

        _expression = ActiveState.Expression;
        RefreshCandidates(preserveActiveState: !IsCartesianActive);
        RaisePropertyChanged(nameof(Expression));
        RaisePropertyChanged(nameof(Display));
        ExpressionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshCandidates(bool preserveActiveState = false)
    {
        var form = Vector2ExpressionCodec.DecodeComponent(
            Expression);
        if (!preserveActiveState || ActiveState is not PolarEditorState)
        {
            SetPolarState(
                Vector2ComponentEditorStateFactory.CreatePolarState(
                    form.Polar));
        }

        if (!preserveActiveState
            || ActiveState is not MemberAccessEditorState)
        {
            SetMemberAccessState(
                Vector2ComponentEditorStateFactory.CreateMemberAccessState(
                    form.MemberAccess));
        }

        if (!preserveActiveState
            || ActiveState is not IndexAccessEditorState)
        {
            SetIndexAccessState(
                Vector2ComponentEditorStateFactory.CreateIndexAccessState(
                    form.IndexAccess));
        }
    }

    private void SetPolarState(PolarEditorState? state)
    {
        if (ReferenceEquals(_polarState, state))
        {
            return;
        }

        Unsubscribe(_polarState);
        _polarState = state;
        Subscribe(_polarState);
        RaisePropertyChanged(nameof(PolarState));
        RaisePropertyChanged(nameof(CanUsePolar));
        RaisePropertyChanged(nameof(IsPolarActive));
    }

    private void SetMemberAccessState(MemberAccessEditorState? state)
    {
        if (ReferenceEquals(_memberAccessState, state))
        {
            return;
        }

        Unsubscribe(_memberAccessState);
        _memberAccessState = state;
        Subscribe(_memberAccessState);
        RaisePropertyChanged(nameof(MemberAccessState));
        RaisePropertyChanged(nameof(CanUseMemberAccess));
        RaisePropertyChanged(nameof(IsMemberAccessActive));
    }

    private void SetIndexAccessState(IndexAccessEditorState? state)
    {
        if (ReferenceEquals(_indexAccessState, state))
        {
            return;
        }

        Unsubscribe(_indexAccessState);
        _indexAccessState = state;
        Subscribe(_indexAccessState);
        RaisePropertyChanged(nameof(IndexAccessState));
        RaisePropertyChanged(nameof(CanUseIndexAccess));
        RaisePropertyChanged(nameof(IsIndexAccessActive));
    }

    private void RaiseModePropertiesChanged()
    {
        RaisePropertyChanged(nameof(IsCartesianActive));
        RaisePropertyChanged(nameof(IsPolarActive));
        RaisePropertyChanged(nameof(IsMemberAccessActive));
        RaisePropertyChanged(nameof(IsIndexAccessActive));
    }

    private void Subscribe(Vector2ComponentEditorState? state)
    {
        if (state is not null)
        {
            state.ExpressionChanged += OnEditorExpressionChanged;
        }
    }

    private void Unsubscribe(Vector2ComponentEditorState? state)
    {
        if (state is not null)
        {
            state.ExpressionChanged -= OnEditorExpressionChanged;
        }
    }

}
