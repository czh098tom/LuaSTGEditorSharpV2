using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LuaSTGEditorSharpV2.WPF.Converter
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class PathValidToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var inValidVisibility = parameter == null ? Visibility.Collapsed : (Visibility)parameter;
            var isVisible = Path.Exists(value?.ToString());
            return isVisible ? Visibility.Visible : inValidVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
