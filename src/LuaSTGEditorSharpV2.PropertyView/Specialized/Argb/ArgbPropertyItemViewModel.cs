using LuaSTGEditorSharpV2.Core.Parsing.Argb;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Argb;

public sealed class ArgbPropertyItemViewModel
    : NamedPropertyItemViewModel<ArgbPropertyItemTerm>
{
    public BoundProperty AlphaProperty { get; } = new();

    public BoundProperty RedProperty { get; } = new();

    public BoundProperty GreenProperty { get; } = new();

    public BoundProperty BlueProperty { get; } = new();

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
        if (ArgbExpressionCodec.Decode(value) is not { } components)
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
}
