using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.WPF.Behavior
{
    public static class EditorVisualBehavior
    {
        private static readonly DependencyPropertyDescriptor ComboBoxTemplateDescriptor =
            DependencyPropertyDescriptor.FromProperty(Control.TemplateProperty, typeof(ComboBox))!;

        public static readonly DependencyProperty FlattenEditableBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "FlattenEditableBackground",
                typeof(bool),
                typeof(EditorVisualBehavior),
                new PropertyMetadata(false, OnFlattenEditableBackgroundChanged));

        private static readonly DependencyProperty IsComboBoxTemplateObserverAttachedProperty =
            DependencyProperty.RegisterAttached(
                "IsComboBoxTemplateObserverAttached",
                typeof(bool),
                typeof(EditorVisualBehavior),
                new PropertyMetadata(false));

        public static void SetFlattenEditableBackground(DependencyObject element, bool value) =>
            element.SetValue(FlattenEditableBackgroundProperty, value);

        public static bool GetFlattenEditableBackground(DependencyObject element) =>
            (bool)element.GetValue(FlattenEditableBackgroundProperty);

        private static void OnFlattenEditableBackgroundChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not ComboBox comboBox)
            {
                return;
            }

            if ((bool)e.OldValue)
            {
                DetachComboBox(comboBox);
            }

            if ((bool)e.NewValue)
            {
                AttachComboBox(comboBox);
            }
        }

        private static void AttachComboBox(ComboBox comboBox)
        {
            comboBox.Loaded += OnComboBoxLoaded;
            comboBox.Unloaded += OnComboBoxUnloaded;
            comboBox.IsEnabledChanged += OnComboBoxIsEnabledChanged;
            if (comboBox.IsLoaded)
            {
                AttachComboBoxTemplateObserver(comboBox);
            }

            UpdateEditableBackground(comboBox);
        }

        private static void DetachComboBox(ComboBox comboBox)
        {
            comboBox.Loaded -= OnComboBoxLoaded;
            comboBox.Unloaded -= OnComboBoxUnloaded;
            comboBox.IsEnabledChanged -= OnComboBoxIsEnabledChanged;
            DetachComboBoxTemplateObserver(comboBox);
            ClearEditableBackground(comboBox);
        }

        private static void OnComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            AttachComboBoxTemplateObserver(comboBox);
            UpdateEditableBackground(comboBox);
        }

        private static void OnComboBoxUnloaded(object sender, RoutedEventArgs e) =>
            DetachComboBoxTemplateObserver((ComboBox)sender);

        private static void OnComboBoxIsEnabledChanged(
            object sender,
            DependencyPropertyChangedEventArgs e) =>
            UpdateEditableBackground((ComboBox)sender);

        private static void AttachComboBoxTemplateObserver(ComboBox comboBox)
        {
            if ((bool)comboBox.GetValue(IsComboBoxTemplateObserverAttachedProperty))
            {
                return;
            }

            ComboBoxTemplateDescriptor.AddValueChanged(comboBox, OnComboBoxTemplateChanged);
            comboBox.SetValue(IsComboBoxTemplateObserverAttachedProperty, true);
        }

        private static void DetachComboBoxTemplateObserver(ComboBox comboBox)
        {
            if (!(bool)comboBox.GetValue(IsComboBoxTemplateObserverAttachedProperty))
            {
                return;
            }

            ComboBoxTemplateDescriptor.RemoveValueChanged(comboBox, OnComboBoxTemplateChanged);
            comboBox.ClearValue(IsComboBoxTemplateObserverAttachedProperty);
        }

        private static void OnComboBoxTemplateChanged(object? sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                UpdateEditableBackground(comboBox);
            }
        }

        private static void UpdateEditableBackground(ComboBox comboBox)
        {
            if (!GetFlattenEditableBackground(comboBox))
            {
                return;
            }

            comboBox.ApplyTemplate();
            if (FindEditableBackground(comboBox) is not Border border)
            {
                return;
            }

            border.SetResourceReference(
                Border.BackgroundProperty,
                comboBox.IsEnabled
                    ? SystemColors.WindowBrushKey
                    : SystemColors.ControlBrushKey);
        }

        private static void ClearEditableBackground(ComboBox comboBox)
        {
            comboBox.ApplyTemplate();
            FindEditableBackground(comboBox)?.ClearValue(Border.BackgroundProperty);
        }

        private static Border? FindEditableBackground(ComboBox comboBox)
        {
            var textBox = FindVisualDescendant<TextBox>(
                comboBox,
                "PART_EditableTextBox");
            return textBox is null
                ? null
                : VisualTreeHelper.GetParent(textBox) as Border;
        }

        private static T? FindVisualDescendant<T>(
            DependencyObject parent,
            string name)
            where T : FrameworkElement
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                if (child is T element && element.Name == name)
                {
                    return element;
                }

                var descendant = FindVisualDescendant<T>(child, name);
                if (descendant is not null)
                {
                    return descendant;
                }
            }

            return null;
        }
    }
}
