using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LuaSTGEditorSharpV2.Core.Parsing.Argb;

namespace LuaSTGEditorSharpV2.PropertyView.Converter;

public sealed record ArgbPreviewResult(
    Color Color, bool IsAvailable, Color OpaquePreview)
{
    public static ArgbPreviewResult Unavailable { get; } =
        new(Colors.Transparent, false, Colors.White);
}

[ValueConversion(typeof(object[]), typeof(ArgbPreviewResult))]
public sealed class ArgbTextToPreviewConverter : IMultiValueConverter
{
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        var preview = CreatePreview(values);
        return preview is { } value
            ? new ArgbPreviewResult(
                Color.FromArgb(
                    value.Alpha,
                    value.Red,
                    value.Green,
                    value.Blue),
                true,
                Color.FromRgb(
                    value.Red,
                    value.Green,
                    value.Blue))
            : ArgbPreviewResult.Unavailable;
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static ArgbPreview? CreatePreview(object[] values)
    {
        if (values is not { Length: 4 })
        {
            return null;
        }

        return ArgbExpressionCodec.CreatePreview(
            new ArgbExpression(
                GetText(values[0]),
                GetText(values[1]),
                GetText(values[2]),
                GetText(values[3])));
    }

    private static string GetText(object value)
        => value as string ?? string.Empty;
}
