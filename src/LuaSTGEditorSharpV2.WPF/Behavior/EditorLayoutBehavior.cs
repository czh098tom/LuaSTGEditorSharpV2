using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.WPF.Behavior
{
    public static class EditorLayoutBehavior
    {
        private const double EditorBorderHeight = 2.0;

        private static readonly DependencyPropertyDescriptor[] ButtonFontDescriptors =
        {
            DependencyPropertyDescriptor.FromProperty(Control.FontFamilyProperty, typeof(Button))!,
            DependencyPropertyDescriptor.FromProperty(Control.FontSizeProperty, typeof(Button))!,
            DependencyPropertyDescriptor.FromProperty(Control.FontStretchProperty, typeof(Button))!,
            DependencyPropertyDescriptor.FromProperty(Control.FontStyleProperty, typeof(Button))!,
            DependencyPropertyDescriptor.FromProperty(Control.FontWeightProperty, typeof(Button))!
        };

        public static readonly DependencyProperty MeasureTextLineHeightProperty =
            DependencyProperty.RegisterAttached(
                "MeasureTextLineHeight",
                typeof(bool),
                typeof(EditorLayoutBehavior),
                new PropertyMetadata(false, OnMeasureTextLineHeightChanged));

        private static readonly DependencyPropertyKey MeasuredEditorHeightPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "MeasuredEditorHeight",
                typeof(double),
                typeof(EditorLayoutBehavior),
                new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MeasuredEditorHeightProperty =
            MeasuredEditorHeightPropertyKey.DependencyProperty;

        private static readonly DependencyProperty IsButtonFontObserverAttachedProperty =
            DependencyProperty.RegisterAttached(
                "IsButtonFontObserverAttached",
                typeof(bool),
                typeof(EditorLayoutBehavior),
                new PropertyMetadata(false));

        public static void SetMeasureTextLineHeight(DependencyObject element, bool value) =>
            element.SetValue(MeasureTextLineHeightProperty, value);

        public static bool GetMeasureTextLineHeight(DependencyObject element) =>
            (bool)element.GetValue(MeasureTextLineHeightProperty);

        public static double GetMeasuredEditorHeight(DependencyObject element) =>
            (double)element.GetValue(MeasuredEditorHeightProperty);

        private static void OnMeasureTextLineHeightChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not Button button)
            {
                return;
            }

            if ((bool)e.OldValue)
            {
                button.Loaded -= OnButtonLoaded;
                button.Unloaded -= OnButtonUnloaded;
                DetachButtonFontObservers(button);
                button.ClearValue(MeasuredEditorHeightPropertyKey);
            }

            if (!(bool)e.NewValue)
            {
                return;
            }

            button.Loaded += OnButtonLoaded;
            button.Unloaded += OnButtonUnloaded;
            if (button.IsLoaded)
            {
                AttachButtonFontObservers(button);
            }

            UpdateMeasuredEditorHeight(button);
        }

        private static void OnButtonLoaded(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            AttachButtonFontObservers(button);
            UpdateMeasuredEditorHeight(button);
        }

        private static void OnButtonUnloaded(object sender, RoutedEventArgs e) =>
            DetachButtonFontObservers((Button)sender);

        private static void AttachButtonFontObservers(Button button)
        {
            if ((bool)button.GetValue(IsButtonFontObserverAttachedProperty))
            {
                return;
            }

            foreach (var descriptor in ButtonFontDescriptors)
            {
                descriptor.AddValueChanged(button, OnButtonFontChanged);
            }

            button.SetValue(IsButtonFontObserverAttachedProperty, true);
        }

        private static void DetachButtonFontObservers(Button button)
        {
            if (!(bool)button.GetValue(IsButtonFontObserverAttachedProperty))
            {
                return;
            }

            foreach (var descriptor in ButtonFontDescriptors)
            {
                descriptor.RemoveValueChanged(button, OnButtonFontChanged);
            }

            button.ClearValue(IsButtonFontObserverAttachedProperty);
        }

        private static void OnButtonFontChanged(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                UpdateMeasuredEditorHeight(button);
            }
        }

        private static void UpdateMeasuredEditorHeight(Button button)
        {
            var probe = new TextBlock
            {
                Text = "M",
                FontFamily = button.FontFamily,
                FontSize = button.FontSize,
                FontStretch = button.FontStretch,
                FontStyle = button.FontStyle,
                FontWeight = button.FontWeight
            };
            TextOptions.SetTextFormattingMode(
                probe,
                TextOptions.GetTextFormattingMode(button));
            probe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            button.SetValue(
                MeasuredEditorHeightPropertyKey,
                probe.DesiredSize.Height + EditorBorderHeight);
        }
    }
}
