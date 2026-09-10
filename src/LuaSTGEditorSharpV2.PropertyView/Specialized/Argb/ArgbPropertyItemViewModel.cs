using System.Windows.Media;
using LuaSTGEditorSharpV2.Core.Parsing.Argb;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Argb;

public sealed class ArgbPropertyItemViewModel
    : NamedPropertyItemViewModel<ArgbPropertyItemTerm>
{
    private bool _previewAvailable;
    private Color? _previewColor;

    public BoundProperty AlphaProperty { get; } = new();

    public BoundProperty RedProperty { get; } = new();

    public BoundProperty GreenProperty { get; } = new();

    public BoundProperty BlueProperty { get; } = new();

    public bool IsPreviewAvailable
        => !AlphaProperty.HasConflict &&
           !RedProperty.HasConflict &&
           !GreenProperty.HasConflict &&
           !BlueProperty.HasConflict &&
           _previewAvailable;

    public Color? PreviewColor
        => IsPreviewAvailable ? _previewColor : null;

    protected override void ConfigureBinding(ArgbPropertyItemTerm term)
    {
        Bind(term.Mapping).ToMany(
            [AlphaProperty, RedProperty, GreenProperty, BlueProperty],
            Compose,
            Decompose);
    }

    private string Compose(string[] values)
        => ArgbExpressionCodec.Compose(new ArgbExpression(
            values[0],
            values[1],
            values[2],
            values[3]));

    private string[] Decompose(string value)
    {
        var form = ArgbExpressionCodec.Decode(value);
        UpdatePreview(form.Preview);

        if (form.Components is not ArgbExpression components)
        {
            return
            [
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
            ];
        }

        return
        [
            components.Alpha,
            components.Red,
            components.Green,
            components.Blue,
        ];
    }

    private void UpdatePreview(ArgbPreview? preview)
    {
        _previewAvailable = preview.HasValue;
        _previewColor = preview is { } value
            ? Color.FromArgb(
                value.Alpha.Value,
                value.Red.Value,
                value.Green.Value,
                value.Blue.Value)
            : null;
    }

    protected override void OnBindingValuesApplied(
        string key,
        PropertyBinding binding,
        string[][] rows)
    {
        RaiseDerivedPropertyChanges();
    }

    private void RaiseDerivedPropertyChanges()
    {
        RaisePropertyChanged(nameof(IsPreviewAvailable));
        RaisePropertyChanged(nameof(PreviewColor));
    }
}
