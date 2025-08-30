using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LuaSTGEditorSharpV2.PropertyView.Editor
{
    public class ComboBoxItemConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ServiceProviderProperty =
            DependencyProperty.Register(
                nameof(ServiceProvider),
                typeof(IServiceProvider),
                typeof(ComboBoxItemConverter),
                new PropertyMetadata(null)
            );

        public IServiceProvider? ServiceProvider
        {
            get => (IServiceProvider?)GetValue(ServiceProviderProperty);
            set => SetValue(ServiceProviderProperty, value);
        }

        private Lazy<ComboboxOptionsServiceProvider> comboboxOptionsServiceProvider;

        public ComboBoxItemConverter()
        {
            comboboxOptionsServiceProvider = new(() =>
            {
                if (ServiceProvider == null)
                    throw new InvalidOperationException("ServiceProvider is not set.");
                return ServiceProvider.GetRequiredService<ComboboxOptionsServiceProvider>();
            });
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return comboboxOptionsServiceProvider.Value.GetComboboxOptions(str).Options
                    .Select(o => o?.Result ?? string.Empty).ToArray();
            }
            if (value is JArray arr)
            {
                return arr.Select(token => token.ToString());
            }
            return Array.Empty<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
