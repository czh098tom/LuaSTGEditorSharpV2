using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core.Parsing.Facade;

namespace LuaSTGEditorSharpV2.Dialog.ViewModel;

public sealed class Vector2EditDialogViewModel : INotifyPropertyChanged
{
    private string _expression = string.Empty;
    private string _x = string.Empty;
    private string _y = string.Empty;
    private VectorComponent? _selectedComponent;
    private string _lastEditedComponent = "Y";
    private bool _synchronizing;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Expression
    {
        get => _expression;
        set
        {
            if (_expression == value)
            {
                return;
            }

            _expression = value;
            RaisePropertyChanged();
            if (!_synchronizing)
            {
                SetComponentsFromExpression(value);
            }
        }
    }

    public string X
    {
        get => _x;
        set
        {
            if (_x == value)
            {
                return;
            }

            _lastEditedComponent = "X";
            _x = value;
            RaisePropertyChanged();
            if (!_synchronizing)
            {
                UpdateExpressionFromComponents();
            }
        }
    }

    public string Y
    {
        get => _y;
        set
        {
            if (_y == value)
            {
                return;
            }

            _lastEditedComponent = "Y";
            _y = value;
            RaisePropertyChanged();
            if (!_synchronizing)
            {
                UpdateExpressionFromComponents();
            }
        }
    }

    public ObservableCollection<VectorComponent> Components { get; } = new();

    public VectorComponent? SelectedComponent
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
            RaisePropertyChanged(nameof(CurrentX));
            RaisePropertyChanged(nameof(CurrentY));
            RaisePropertyChanged(nameof(CurrentRadius));
            RaisePropertyChanged(nameof(CurrentAngle));
            RaisePropertyChanged(nameof(IsCurrentCartesian));
            RaisePropertyChanged(nameof(IsCurrentPolar));
            RaisePropertyChanged(nameof(CanCurrentUsePolar));
        }
    }

    public string CurrentX
    {
        get => SelectedComponent?.X ?? string.Empty;
        set
        {
            if (SelectedComponent is null || SelectedComponent.X == value)
            {
                return;
            }

            _lastEditedComponent = "X";
            SelectedComponent.X = value;
            MergeX();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Expression));
        }
    }

    public string CurrentY
    {
        get => SelectedComponent?.Y ?? string.Empty;
        set
        {
            if (SelectedComponent is null || SelectedComponent.Y == value)
            {
                return;
            }

            _lastEditedComponent = "Y";
            SelectedComponent.Y = value;
            MergeY();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Expression));
        }
    }

    public string CurrentRadius
    {
        get => SelectedComponent?.Radius ?? string.Empty;
        set
        {
            if (SelectedComponent is null || SelectedComponent.Radius == value)
            {
                return;
            }

            SelectedComponent.SetPolarValues(value, SelectedComponent.Angle);
            MergePolarComponent();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CurrentX));
            RaisePropertyChanged(nameof(CurrentY));
        }
    }

    public string CurrentAngle
    {
        get => SelectedComponent?.Angle ?? string.Empty;
        set
        {
            if (SelectedComponent is null || SelectedComponent.Angle == value)
            {
                return;
            }

            SelectedComponent.SetPolarValues(SelectedComponent.Radius, value);
            MergePolarComponent();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CurrentX));
            RaisePropertyChanged(nameof(CurrentY));
        }
    }

    public bool IsCurrentCartesian
    {
        get => SelectedComponent?.Mode != VectorComponentEditMode.Polar;
        set
        {
            if (value)
            {
                SetCurrentMode(VectorComponentEditMode.Cartesian);
            }
        }
    }

    public bool IsCurrentPolar
    {
        get => SelectedComponent?.Mode == VectorComponentEditMode.Polar;
        set
        {
            if (value)
            {
                SetCurrentMode(VectorComponentEditMode.Polar);
            }
        }
    }

    public bool CanCurrentUsePolar => SelectedComponent?.CanUsePolar == true;

    public ICommand SyncXYCommand { get; }
    public ICommand SyncTrigonometricsCommand { get; }

    public Vector2EditDialogViewModel()
    {
        SyncXYCommand = new RelayCommand(SyncXY);
        SyncTrigonometricsCommand = new RelayCommand(SyncTrigonometrics);
        SetInitialValues(string.Empty, string.Empty);
    }

    public void SetInitialValues(string x, string y)
    {
        _synchronizing = true;
        try
        {
            _x = x;
            _y = y;
            _expression = Vector2EditHelper.Compose(x, y);
        }
        finally
        {
            _synchronizing = false;
        }

        RebuildComponents();
        RaisePropertyChanged(nameof(X));
        RaisePropertyChanged(nameof(Y));
        RaisePropertyChanged(nameof(Expression));
    }

    public void AppendVector(double x, double y)
    {
        var xText = FormatNumber(x);
        var yText = FormatNumber(y);
        X = AppendTerm(X, xText);
        Y = AppendTerm(Y, yText);
    }

    private void SetComponentsFromExpression(string expression)
    {
        var components = Vector2EditHelper.Decompose(expression);
        _synchronizing = true;
        try
        {
            _x = components.Item1;
            _y = components.Item2;
        }
        finally
        {
            _synchronizing = false;
        }

        RebuildComponents();
        RaisePropertyChanged(nameof(X));
        RaisePropertyChanged(nameof(Y));
    }

    private void UpdateExpressionFromComponents()
    {
        _synchronizing = true;
        try
        {
            _expression = Vector2EditHelper.Compose(_x, _y);
        }
        finally
        {
            _synchronizing = false;
        }

        RebuildComponents();
        RaisePropertyChanged(nameof(Expression));
    }

    private void RebuildComponents()
    {
        var selectedIndex = SelectedComponent is null ? -1 : Components.IndexOf(SelectedComponent);
        var previousModes = Components.Select(component => component.Mode).ToArray();
        Components.Clear();

        var xTerms = SeparatePolynomial(_x);
        var yTerms = SeparatePolynomial(_y);
        var count = Math.Max(xTerms.Count, yTerms.Count);
        for (var i = 0; i < count; i++)
        {
            var component = new VectorComponent(
                i < xTerms.Count ? xTerms[i] : "0",
                i < yTerms.Count ? yTerms[i] : "0");
            if (i < previousModes.Length
                && previousModes[i] == VectorComponentEditMode.Polar
                && component.CanUsePolar)
            {
                component.Mode = VectorComponentEditMode.Polar;
            }

            Components.Add(component);
        }

        SelectedComponent = selectedIndex >= 0 && selectedIndex < Components.Count
            ? Components[selectedIndex]
            : null;
    }

    private void MergeX()
    {
        SetComponentValue(isX: true, MergeComponent(component => component.X));
    }

    private void MergeY()
    {
        SetComponentValue(isX: false, MergeComponent(component => component.Y));
    }

    private void MergePolarComponent()
    {
        _synchronizing = true;
        try
        {
            _x = MergeComponent(component => component.X);
            _y = MergeComponent(component => component.Y);
            _expression = Vector2EditHelper.Compose(_x, _y);
        }
        finally
        {
            _synchronizing = false;
        }

        RaisePropertyChanged(nameof(X));
        RaisePropertyChanged(nameof(Y));
        RaisePropertyChanged(nameof(Expression));
    }

    private void SetCurrentMode(VectorComponentEditMode mode)
    {
        if (SelectedComponent is null || SelectedComponent.Mode == mode)
        {
            return;
        }

        SelectedComponent.Mode = mode;
        RaisePropertyChanged(nameof(IsCurrentCartesian));
        RaisePropertyChanged(nameof(IsCurrentPolar));
        RaisePropertyChanged(nameof(CurrentRadius));
        RaisePropertyChanged(nameof(CurrentAngle));
    }

    private void SetComponentValue(bool isX, string value)
    {
        _synchronizing = true;
        try
        {
            if (isX)
            {
                _x = value;
            }
            else
            {
                _y = value;
            }
            _expression = Vector2EditHelper.Compose(_x, _y);
        }
        finally
        {
            _synchronizing = false;
        }

        RaisePropertyChanged(isX ? nameof(X) : nameof(Y));
        RebuildComponents();
    }

    private string MergeComponent(Func<VectorComponent, string> selector)
    {
        var lastNonZero = Components.Count - 1;
        while (lastNonZero >= 0 && IsEmpty(selector(Components[lastNonZero])))
        {
            lastNonZero--;
        }

        if (lastNonZero < 0)
        {
            return string.Empty;
        }

        var result = string.Empty;
        for (var i = 0; i <= lastNonZero; i++)
        {
            var term = selector(Components[i]).Trim();
            if (term.Length == 0)
            {
                term = "0";
            }

            if (result.Length > 0 && term[0] != '-')
            {
                result += "+";
            }
            result += term;
        }

        return result;
    }

    private void SyncXY()
    {
        if (SelectedComponent is null)
        {
            return;
        }

        if (IsEmpty(CurrentX) && IsEmpty(CurrentY))
        {
            return;
        }

        if (IsEmpty(CurrentX) || (!IsEmpty(CurrentY) && _lastEditedComponent == "Y"))
        {
            CurrentX = SwapAxes(CurrentY);
        }
        else
        {
            CurrentY = SwapAxes(CurrentX);
        }
    }

    private void SyncTrigonometrics()
    {
        if (SelectedComponent is null)
        {
            return;
        }

        if (IsEmpty(CurrentX) && IsEmpty(CurrentY))
        {
            return;
        }

        if (IsEmpty(CurrentX) || (!IsEmpty(CurrentY) && _lastEditedComponent == "Y"))
        {
            CurrentX = SwapTrigonometrics(CurrentY);
        }
        else
        {
            CurrentY = SwapTrigonometrics(CurrentX);
        }
    }

    private static string SwapAxes(string value)
    {
        const string marker = "____TEMP_Y____";
        var result = Regex.Replace(value, "(?<![a-zA-Z])x\\b", marker);
        result = Regex.Replace(result, "(?<![a-zA-Z])y\\b", "x");
        return Regex.Replace(result, "(?<![a-zA-Z])" + marker + "\\b", "y");
    }

    private static string SwapTrigonometrics(string value)
    {
        const string marker = "____TEMP_SIN____(";
        var result = Regex.Replace(value, "\\bsin\\(", marker);
        result = Regex.Replace(result, "\\bcos\\(", "sin(");
        return Regex.Replace(result, "____TEMP_SIN____\\(", "cos(");
    }

    private static string AppendTerm(string value, string term)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return term;
        }

        return term.StartsWith('-') ? value + term : value + "+" + term;
    }

    private static bool IsEmpty(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return double.TryParse(value, out var number) && number == 0;
    }

    private static string FormatNumber(double value)
        => value.ToString("G", System.Globalization.CultureInfo.CurrentCulture);

    private static List<string> SeparatePolynomial(string value)
    {
        var result = new List<string>();
        var depth = 0;
        var start = 0;

        for (var i = 0; i < value.Length; i++)
        {
            switch (value[i])
            {
                case '(':
                case '[':
                case '{':
                    depth++;
                    break;
                case ')':
                case ']':
                case '}':
                    depth = Math.Max(0, depth - 1);
                    break;
                case '+':
                    if (depth == 0 && i > start)
                    {
                        AddTerm(result, value[start..i]);
                        start = i + 1;
                    }
                    break;
                case '-':
                    if (depth == 0 && i > start && IsBinaryMinus(value, i))
                    {
                        AddTerm(result, value[start..i]);
                        start = i;
                    }
                    break;
            }
        }

        if (start < value.Length)
        {
            AddTerm(result, value[start..]);
        }

        return result;
    }

    private static bool IsBinaryMinus(string value, int index)
    {
        for (var i = index - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(value[i]))
            {
                continue;
            }

            return value[i] is not ('+' or '-' or '*' or '/' or '%' or '^' or '(' or '[' or '{' or ',');
        }

        return false;
    }

    private static void AddTerm(List<string> terms, string term)
    {
        terms.Add(term.Trim());
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public enum VectorComponentEditMode
{
    Cartesian,
    Polar,
}

public sealed class VectorComponent : INotifyPropertyChanged
{
    private string _x;
    private string _y;
    private string _radius = string.Empty;
    private string _angle = string.Empty;
    private VectorComponentEditMode _mode;
    private bool _canUsePolar;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string X
    {
        get => _x;
        set
        {
            if (_x == value)
            {
                return;
            }

            _x = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Display));
            RefreshPolarState();
        }
    }

    public string Y
    {
        get => _y;
        set
        {
            if (_y == value)
            {
                return;
            }

            _y = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Display));
            RefreshPolarState();
        }
    }

    public string Radius => _radius;

    public string Angle => _angle;

    public VectorComponentEditMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value || value == VectorComponentEditMode.Polar && !CanUsePolar)
            {
                return;
            }

            _mode = value;
            RaisePropertyChanged();
        }
    }

    public bool CanUsePolar
    {
        get => _canUsePolar;
        private set
        {
            if (_canUsePolar == value)
            {
                return;
            }

            _canUsePolar = value;
            RaisePropertyChanged();
        }
    }

    public string Display => $"({_x}, {_y})";

    public VectorComponent(string x, string y)
    {
        _x = x;
        _y = y;
        RefreshPolarState();
    }

    public void SetPolarValues(string radius, string angle)
    {
        var radiusChanged = _radius != radius;
        var angleChanged = _angle != angle;
        if (!radiusChanged && !angleChanged)
        {
            return;
        }

        _radius = radius;
        _angle = angle;
        CanUsePolar = true;
        Mode = VectorComponentEditMode.Polar;

        var components = PolarVectorExpressionParser.Compose(radius, angle);
        _x = components.X;
        _y = components.Y;

        if (radiusChanged)
        {
            RaisePropertyChanged(nameof(Radius));
        }

        if (angleChanged)
        {
            RaisePropertyChanged(nameof(Angle));
        }

        RaisePropertyChanged(nameof(X));
        RaisePropertyChanged(nameof(Y));
        RaisePropertyChanged(nameof(Display));
    }

    private void RefreshPolarState()
    {
        if (PolarVectorExpressionParser.TryDecompose(
                _x,
                _y,
                out var radius,
                out var angle))
        {
            CanUsePolar = true;
            SetPolarExpression(radius, angle);
            return;
        }

        CanUsePolar = false;
        SetPolarExpression(string.Empty, string.Empty);
        if (_mode == VectorComponentEditMode.Polar)
        {
            _mode = VectorComponentEditMode.Cartesian;
            RaisePropertyChanged(nameof(Mode));
        }
    }

    private void SetPolarExpression(string radius, string angle)
    {
        if (_radius != radius)
        {
            _radius = radius;
            RaisePropertyChanged(nameof(Radius));
        }

        if (_angle != angle)
        {
            _angle = angle;
            RaisePropertyChanged(nameof(Angle));
        }
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
