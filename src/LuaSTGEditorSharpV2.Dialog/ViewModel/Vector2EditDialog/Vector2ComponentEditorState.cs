using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core.Parsing.Vector2;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Dialog.ViewModel.Vector2EditDialog;

public abstract class Vector2ComponentEditorState : ViewModelBase
{
    internal event EventHandler? ExpressionChanged;

    public abstract Vector2ExpressionPair Expression { get; }

    protected bool SetField(
        ref string field,
        string value,
        [CallerMemberName] string propertyName = "")
    {
        value ??= string.Empty;
        if (field == value)
        {
            return false;
        }

        field = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    protected void RaiseExpressionChanged()
        => ExpressionChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class CartesianEditorState
    : Vector2ComponentEditorState
{
    private string _x;
    private string _y;
    private string _lastEditedAxis = "Y";
    private bool _updating;

    public string X
    {
        get => _x;
        set => SetAxis(ref _x, value, "X");
    }

    public string Y
    {
        get => _y;
        set => SetAxis(ref _y, value, "Y");
    }

    public ICommand SyncXYCommand { get; }

    public ICommand SyncTrigonometricsCommand { get; }

    public override Vector2ExpressionPair Expression => new(X, Y);

    public CartesianEditorState(string x, string y)
    {
        _x = x;
        _y = y;
        SyncXYCommand = new RelayCommand(SyncXY);
        SyncTrigonometricsCommand = new RelayCommand(SyncTrigonometrics);
    }

    internal void SetExpression(Vector2ExpressionPair expression)
    {
        _updating = true;
        try
        {
            SetField(ref _x, expression.X, nameof(X));
            SetField(ref _y, expression.Y, nameof(Y));
        }
        finally
        {
            _updating = false;
        }
    }

    private void SetAxis(
        ref string field,
        string value,
        string axis)
    {
        if (!SetField(ref field, value, axis))
        {
            return;
        }

        _lastEditedAxis = axis;
        if (!_updating)
        {
            RaiseExpressionChanged();
        }
    }

    private void SyncXY()
    {
        if (IsEmpty(X) && IsEmpty(Y))
        {
            return;
        }

        if (IsEmpty(X) || !IsEmpty(Y) && _lastEditedAxis == "Y")
        {
            X = SwapAxes(Y);
        }
        else
        {
            Y = SwapAxes(X);
        }
    }

    private void SyncTrigonometrics()
    {
        if (IsEmpty(X) && IsEmpty(Y))
        {
            return;
        }

        if (IsEmpty(X) || !IsEmpty(Y) && _lastEditedAxis == "Y")
        {
            X = SwapTrigonometrics(Y);
        }
        else
        {
            Y = SwapTrigonometrics(X);
        }
    }

    private static string SwapAxes(string value)
    {
        const string marker = "____TEMP_Y____";
        var result = Regex.Replace(value, "(?<![a-zA-Z])x\\b", marker);
        result = Regex.Replace(result, "(?<![a-zA-Z])y\\b", "x");
        return Regex.Replace(
            result,
            "(?<![a-zA-Z])" + marker + "\\b",
            "y");
    }

    private static string SwapTrigonometrics(string value)
    {
        const string marker = "____TEMP_SIN____(";
        var result = Regex.Replace(value, "\\bsin\\(", marker);
        result = Regex.Replace(result, "\\bcos\\(", "sin(");
        return Regex.Replace(result, "____TEMP_SIN____\\(", "cos(");
    }

    private static bool IsEmpty(string? value)
        => string.IsNullOrWhiteSpace(value)
           || double.TryParse(value, out var number)
           && number == 0;
}

public sealed class PolarEditorState
    : Vector2ComponentEditorState
{
    private string _radius;
    private string _angle;

    public string Radius
    {
        get => _radius;
        set
        {
            if (SetField(ref _radius, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public string Angle
    {
        get => _angle;
        set
        {
            if (SetField(ref _angle, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public override Vector2ExpressionPair Expression
        => Vector2ExpressionCodec.Polar.Compose(Radius, Angle);

    public PolarEditorState(string radius, string angle)
    {
        _radius = radius;
        _angle = angle;
    }
}

public sealed class MemberAccessEditorState
    : Vector2ComponentEditorState
{
    private string _source;
    private string _firstAccessor;
    private string _secondAccessor;

    public string Source
    {
        get => _source;
        set
        {
            if (SetField(ref _source, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public string FirstAccessor
    {
        get => _firstAccessor;
        set
        {
            if (SetField(ref _firstAccessor, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public string SecondAccessor
    {
        get => _secondAccessor;
        set
        {
            if (SetField(ref _secondAccessor, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public override Vector2ExpressionPair Expression
        => Vector2ExpressionCodec.MemberAccess.Compose(
            Source,
            FirstAccessor,
            SecondAccessor);

    public MemberAccessEditorState(
        string source,
        string firstAccessor,
        string secondAccessor)
    {
        _source = source;
        _firstAccessor = firstAccessor;
        _secondAccessor = secondAccessor;
    }
}

public sealed class IndexAccessEditorState
    : Vector2ComponentEditorState
{
    private string _source;
    private string _firstAccessor;
    private string _secondAccessor;

    public override Vector2ExpressionPair Expression
        => Vector2ExpressionCodec.IndexAccess.Compose(
            Source,
            FirstAccessor,
            SecondAccessor);

    public string Source
    {
        get => _source;
        set
        {
            if (SetField(ref _source, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public string FirstAccessor
    {
        get => _firstAccessor;
        set
        {
            if (SetField(ref _firstAccessor, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public string SecondAccessor
    {
        get => _secondAccessor;
        set
        {
            if (SetField(ref _secondAccessor, value))
            {
                RaiseExpressionChanged();
            }
        }
    }

    public IndexAccessEditorState(
        string source,
        string firstAccessor,
        string secondAccessor)
    {
        _source = source;
        _firstAccessor = firstAccessor;
        _secondAccessor = secondAccessor;
    }
}
