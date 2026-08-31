using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace LuaSTGEditorSharpV2.PropertyView.Converter;

public sealed class ConflictsToEnabledConverter : IMultiValueConverter
{
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return values.Length > 0 &&
               values.All(value => value is false);
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
