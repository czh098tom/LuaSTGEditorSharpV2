using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core.Parsing.Vector2;
using LuaSTGEditorSharpV2.Dialog.ViewModel.Vector2EditDialog;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.WPF.Behavior;

namespace LuaSTGEditorSharpV2.Dialog.ViewModel;

public sealed class Vector2EditDialogViewModel : ViewModelBase
{
    private string _expression = string.Empty;
    private string _x = string.Empty;
    private string _y = string.Empty;
    private Vector2ComponentViewModel? _selectedComponent;

    public event Action? VectorEditorAppended;

    public string Expression
    {
        get => _expression;
        set
        {
            if (_expression != value)
            {
                LoadExpression(value, preserveSelection: true);
            }
        }
    }

    public string X
    {
        get => _x;
        set
        {
            if (_x != value)
            {
                LoadExpression(
                    Vector2ExpressionCodec.Compose(value, _y),
                    preserveSelection: true);
            }
        }
    }

    public string Y
    {
        get => _y;
        set
        {
            if (_y != value)
            {
                LoadExpression(
                    Vector2ExpressionCodec.Compose(_x, value),
                    preserveSelection: true);
            }
        }
    }

    public ObservableCollection<Vector2ComponentViewModel> Components
    {
        get;
    } = new();

    public Vector2ComponentViewModel? SelectedComponent
    {
        get => _selectedComponent;
        set
        {
            if (ReferenceEquals(_selectedComponent, value))
            {
                return;
            }

            _selectedComponent = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasSelectedComponent));
            RemoveComponentCommand.NotifyCanExecuteChanged();
        }
    }

    public bool HasSelectedComponent => SelectedComponent is not null;

    public ICommand OpenVectorEditorCommand { get; }

    public IRelayCommand AddCartesianComponentCommand { get; }

    public IRelayCommand AddPolarComponentCommand { get; }

    public IRelayCommand AddMemberAccessComponentCommand { get; }

    public IRelayCommand RemoveComponentCommand { get; }

    public IRelayCommand<ItemsControlReorderRequest>
        ReorderComponentCommand { get; }

    public Vector2EditDialogViewModel()
    {
        OpenVectorEditorCommand = new RelayCommand(OpenVectorEditor);
        AddCartesianComponentCommand = new RelayCommand(AddCartesianComponent);
        AddPolarComponentCommand = new RelayCommand(AddPolarComponent);
        AddMemberAccessComponentCommand = new RelayCommand(
            AddMemberAccessComponent);
        RemoveComponentCommand = new RelayCommand(
            RemoveSelectedComponent,
            () => SelectedComponent is not null);
        ReorderComponentCommand =
            new RelayCommand<ItemsControlReorderRequest>(
                ReorderComponent,
                request => request?.Item is Vector2ComponentViewModel);
        SetInitialValues(string.Empty, string.Empty);
    }

    public void SetInitialValues(string x, string y)
        => LoadExpression(
            Vector2ExpressionCodec.Compose(x, y),
            preserveSelection: false);

    public void AppendVector(double x, double y)
        => LoadExpression(
            Vector2ExpressionCodec.Compose(
                AppendTerm(X, FormatNumber(x)),
                AppendTerm(Y, FormatNumber(y))),
            preserveSelection: true);

    public void MoveComponent(
        Vector2ComponentViewModel component,
        int insertionIndex)
    {
        var sourceIndex = Components.IndexOf(component);
        if (sourceIndex < 0)
        {
            return;
        }

        insertionIndex = Math.Clamp(insertionIndex, 0, Components.Count);
        if (sourceIndex < insertionIndex)
        {
            insertionIndex--;
        }

        if (sourceIndex != insertionIndex)
        {
            Components.Move(sourceIndex, insertionIndex);
            MergeComponents();
        }

        SelectedComponent = component;
    }

    private void AddCartesianComponent()
        => AddComponent(
            Vector2ComponentViewModel.CreateCartesian("0", "0"));

    private void AddPolarComponent()
        => AddComponent(
            Vector2ComponentViewModel.CreatePolar("r", "a"));

    private void AddMemberAccessComponent()
        => AddComponent(
            Vector2ComponentViewModel.CreateMemberAccess(
                "self",
                "x",
                "y"));

    private void AddComponent(Vector2ComponentViewModel component)
    {
        Subscribe(component);
        Components.Add(component);
        SelectedComponent = component;
        MergeComponents();
    }

    private void RemoveSelectedComponent()
    {
        if (SelectedComponent is null)
        {
            return;
        }

        var removedIndex = Components.IndexOf(SelectedComponent);
        if (removedIndex < 0)
        {
            SelectedComponent = null;
            return;
        }

        Unsubscribe(SelectedComponent);
        Components.RemoveAt(removedIndex);
        SelectedComponent = Components.Count == 0
            ? null
            : Components[Math.Min(removedIndex, Components.Count - 1)];
        MergeComponents();
    }

    private void ReorderComponent(ItemsControlReorderRequest? request)
    {
        if (request?.Item is Vector2ComponentViewModel component)
        {
            MoveComponent(component, request.InsertionIndex);
        }
    }

    private void LoadExpression(
        string expression,
        bool preserveSelection)
    {
        var selectedIndex = preserveSelection
            && SelectedComponent is not null
                ? Components.IndexOf(SelectedComponent)
                : -1;
        var form = Vector2ExpressionCodec.Decode(expression);

        _expression = expression;
        _x = form.Cartesian.X;
        _y = form.Cartesian.Y;
        ReplaceComponents(form.Components, selectedIndex);

        RaisePropertyChanged(nameof(Expression));
        RaisePropertyChanged(nameof(X));
        RaisePropertyChanged(nameof(Y));
    }

    private void ReplaceComponents(
        IEnumerable<Vector2ComponentForm> forms,
        int selectedIndex)
    {
        foreach (var component in Components)
        {
            Unsubscribe(component);
        }

        Components.Clear();
        foreach (var form in forms)
        {
            var component = Vector2ComponentViewModel.Create(form);
            Subscribe(component);
            Components.Add(component);
        }

        SelectedComponent = selectedIndex >= 0
            && selectedIndex < Components.Count
                ? Components[selectedIndex]
                : null;
    }

    private void OnComponentExpressionChanged(object? sender, EventArgs e)
        => MergeComponents();

    private void MergeComponents()
    {
        var expression = Vector2ExpressionCodec.ComposeComponents(
            Components.Select(component => component.Expression));
        _x = expression.X;
        _y = expression.Y;
        _expression = Vector2ExpressionCodec.Compose(expression);
        RaisePropertyChanged(nameof(X));
        RaisePropertyChanged(nameof(Y));
        RaisePropertyChanged(nameof(Expression));
    }

    private void Subscribe(Vector2ComponentViewModel component)
        => component.ExpressionChanged += OnComponentExpressionChanged;

    private void Unsubscribe(Vector2ComponentViewModel component)
        => component.ExpressionChanged -= OnComponentExpressionChanged;

    private void OpenVectorEditor()
    {
        var editor = new LuaSTGEditorSharpV2.Dialog.VectorEditorDialog
        {
            Owner = Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.IsActive),
        };

        if (editor.ShowDialog() == true)
        {
            AppendVector(editor.OffsetX, editor.OffsetY);
            VectorEditorAppended?.Invoke();
        }
    }

    private static string AppendTerm(string value, string term)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return term;
        }

        return term.StartsWith('-') ? value + term : value + "+" + term;
    }

    private static string FormatNumber(double value)
        => value.ToString("G", CultureInfo.CurrentCulture);

}
