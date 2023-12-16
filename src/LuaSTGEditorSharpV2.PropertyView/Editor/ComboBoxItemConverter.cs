using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LuaSTGEditorSharpV2.PropertyView.Editor
{
    public class ComboBoxItemConverter : IValueConverter
    {
        private static readonly Dictionary<string, string[]> _comboboxTerms = new()
            { {"boolean", ["true", "false"]} };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _comboboxTerms.GetValueOrDefault(value?.ToString() ?? string.Empty, Array.Empty<string>());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
