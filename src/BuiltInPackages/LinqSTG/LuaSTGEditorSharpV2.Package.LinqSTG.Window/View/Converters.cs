using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View
{
    /// <summary>
    /// Resolves a variable list entry's <c>TypeIndex</c> (0 = float, 1 = int,
    /// 2 = vector2) into its localized display name, used by the locked rows'
    /// read-only type label.
    /// </summary>
    public sealed class VariableTypeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int index ? index switch
            {
                1 => Resolve("linqstg_window_node_int", "Int"),
                2 => Resolve("linqstg_window_node_vector2", "Vector2"),
                _ => Resolve("linqstg_window_node_float", "Float"),
            } : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException("The variable type label is one-way.");

        private static string Resolve(string key, string fallback)
            => Localized.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? fallback;
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
