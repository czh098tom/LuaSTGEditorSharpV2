using System;
using System.Globalization;
using System.Windows.Data;

namespace LuaSTGEditorSharpV2.PropertyView.Converter;

public class BooleanConflictConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is true)
        {
            return null;
        }

        return values[0] is string value && bool.TryParse(value, out var result)
            ? result
            : null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        if (value is bool result)
        {
            return [result ? "true" : "false", Binding.DoNothing];
        }

        return [Binding.DoNothing, Binding.DoNothing];
    }
}
