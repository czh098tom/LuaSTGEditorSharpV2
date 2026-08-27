using System;
using System.Globalization;
using System.Windows.Data;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View
{
    /// <summary>
    /// Maps between a boolean choice and the index of the matching item in a
    /// two-item selector (index 0 = false, index 1 = true), used by the
    /// variable list's float/int type picker.
    /// </summary>
    public sealed class BooleanToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? 1 : 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is 1;
    }

    /// <summary>Inverts a boolean, used to disable the delete button of locked entries.</summary>
    public sealed class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is not true;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is not true;
    }
}
