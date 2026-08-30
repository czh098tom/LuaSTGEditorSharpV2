using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LuaSTGEditorSharpV2.WPF.Controls;
using Xceed.Wpf.Toolkit;

namespace LuaSTGEditorSharpV2.PropertyView;

public static class BoundEditor
{
    private static readonly IValueConverter ConflictToEnabledConverter =
        new ConflictToEnabledValueConverter();

    private static readonly IMultiValueConverter ConflictWatermarkConverter =
        new ConflictWatermarkValueConverter();

    public static readonly DependencyProperty PropertyProperty =
        DependencyProperty.RegisterAttached(
            "Property",
            typeof(BoundProperty),
            typeof(BoundEditor),
            new PropertyMetadata(null, OnPropertyChanged));

    public static void SetProperty(
        DependencyObject editor,
        BoundProperty? value)
    {
        editor.SetValue(PropertyProperty, value);
    }

    public static BoundProperty? GetProperty(DependencyObject editor)
    {
        return (BoundProperty?)editor.GetValue(PropertyProperty);
    }

    public static readonly DependencyProperty ConflictTextProperty =
        DependencyProperty.RegisterAttached(
            "ConflictText",
            typeof(object),
            typeof(BoundEditor),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits));

    public static void SetConflictText(
        DependencyObject element,
        object? value)
    {
        element.SetValue(ConflictTextProperty, value);
    }

    public static object? GetConflictText(DependencyObject element)
    {
        return element.GetValue(ConflictTextProperty);
    }

    private static void OnPropertyChanged(
        DependencyObject editor,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        var adapter = GetAdapter(editor);
        ClearBindings(editor, adapter);

        if (eventArgs.NewValue is BoundProperty property)
        {
            ApplyBindings(editor, adapter, property);
        }
    }

    private static void ApplyBindings(
        DependencyObject editor,
        EditorAdapter adapter,
        BoundProperty property)
    {
        BindingOperations.SetBinding(
            editor,
            adapter.TextProperty,
            new Binding(nameof(BoundProperty.Value))
            {
                Source = property,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
            });

        var watermarkBinding = new MultiBinding
        {
            Mode = BindingMode.OneWay,
            Converter = ConflictWatermarkConverter,
        };
        watermarkBinding.Bindings.Add(
            new Binding(nameof(BoundProperty.HasConflict))
            {
                Source = property,
                Mode = BindingMode.OneWay,
            });
        watermarkBinding.Bindings.Add(
            new Binding
            {
                Source = editor,
                Path = new PropertyPath(ConflictTextProperty),
                Mode = BindingMode.OneWay,
            });
        BindingOperations.SetBinding(
            editor,
            adapter.WatermarkProperty,
            watermarkBinding);

        BindingOperations.SetBinding(
            editor,
            UIElement.IsEnabledProperty,
            new Binding(nameof(BoundProperty.HasConflict))
            {
                Source = property,
                Mode = BindingMode.OneWay,
                Converter = ConflictToEnabledConverter,
            });
    }

    private static void ClearBindings(
        DependencyObject editor,
        EditorAdapter adapter)
    {
        BindingOperations.ClearBinding(editor, adapter.TextProperty);
        BindingOperations.ClearBinding(editor, adapter.WatermarkProperty);
        BindingOperations.ClearBinding(editor, UIElement.IsEnabledProperty);
    }

    private static EditorAdapter GetAdapter(DependencyObject editor)
    {
        return editor switch
        {
            WatermarkTextBox => TextBoxAdapter,
            WatermarkComboBox => ComboBoxAdapter,
            ExpressionSpinner => ExpressionSpinnerAdapter,
            _ => throw new InvalidOperationException(
                $"{nameof(BoundEditor)} does not support editor type " +
                $"{editor.GetType().FullName}."),
        };
    }

    private static readonly EditorAdapter TextBoxAdapter =
        new(TextBox.TextProperty, WatermarkTextBox.WatermarkProperty);

    private static readonly EditorAdapter ComboBoxAdapter =
        new(ComboBox.TextProperty, WatermarkComboBox.WatermarkProperty);

    private static readonly EditorAdapter ExpressionSpinnerAdapter =
        new(
            ExpressionSpinner.TextProperty,
            ExpressionSpinner.WatermarkProperty);

    private sealed record EditorAdapter(
        DependencyProperty TextProperty,
        DependencyProperty WatermarkProperty);

    private sealed class ConflictToEnabledValueConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value is not true;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class ConflictWatermarkValueConverter : IMultiValueConverter
    {
        public object? Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return values.Length >= 2 && values[0] is true
                ? values[1]
                : null;
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
}
