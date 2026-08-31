using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using Xceed.Wpf.Toolkit;

namespace LuaSTGEditorSharpV2.WPF.Behavior
{
    public static class EditorVisualBehavior
    {
        public static readonly DependencyProperty EnabledBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "EnabledBackground",
                typeof(Brush),
                typeof(EditorVisualBehavior),
                new PropertyMetadata(null, OnBackgroundChanged));

        public static readonly DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "DisabledBackground",
                typeof(Brush),
                typeof(EditorVisualBehavior),
                new PropertyMetadata(null, OnBackgroundChanged));

        private static readonly DependencyProperty BackgroundBehaviorProperty =
            DependencyProperty.RegisterAttached(
                "BackgroundBehavior",
                typeof(ControlBackgroundBehavior),
                typeof(EditorVisualBehavior));

        public static void SetEnabledBackground(
            DependencyObject element,
            Brush? value) =>
            element.SetValue(EnabledBackgroundProperty, value);

        public static Brush? GetEnabledBackground(DependencyObject element) =>
            (Brush?)element.GetValue(EnabledBackgroundProperty);

        public static void SetDisabledBackground(
            DependencyObject element,
            Brush? value) =>
            element.SetValue(DisabledBackgroundProperty, value);

        public static Brush? GetDisabledBackground(DependencyObject element) =>
            (Brush?)element.GetValue(DisabledBackgroundProperty);

        private static void OnBackgroundChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is not Control control)
            {
                return;
            }

            var behavior = (ControlBackgroundBehavior?)
                control.GetValue(BackgroundBehaviorProperty);
            var shouldAttach =
                GetEnabledBackground(control) is not null ||
                GetDisabledBackground(control) is not null;

            if (shouldAttach && behavior is null)
            {
                behavior = new ControlBackgroundBehavior();
                Interaction.GetBehaviors(control).Add(behavior);
                control.SetValue(BackgroundBehaviorProperty, behavior);
            }

            if (!shouldAttach && behavior is not null)
            {
                control.ClearValue(BackgroundBehaviorProperty);
                Interaction.GetBehaviors(control).Remove(behavior);
                return;
            }

            behavior?.UpdateBackground();
        }

        private sealed class ControlBackgroundBehavior : Behavior<Control>
        {
            protected override void OnAttached()
            {
                base.OnAttached();
                AssociatedObject.IsEnabledChanged += OnIsEnabledChanged;
                UpdateBackground();
            }

            protected override void OnDetaching()
            {
                AssociatedObject.IsEnabledChanged -= OnIsEnabledChanged;
                ClearBackground();
                base.OnDetaching();
            }

            internal void UpdateBackground()
            {
                var background = AssociatedObject.IsEnabled
                    ? GetEnabledBackground(AssociatedObject)
                    : GetDisabledBackground(AssociatedObject);

                if (background is null)
                {
                    ClearBackground();
                }
                else
                {
                    SetBackground(background);
                }
            }

            private void OnIsEnabledChanged(
                object sender,
                DependencyPropertyChangedEventArgs eventArgs) =>
                UpdateBackground();

            private void SetBackground(Brush? background)
            {
                if (background is null)
                {
                    ClearBackground();
                }
                else
                {
                    AssociatedObject.SetValue(Control.BackgroundProperty, background);
                    // investigated that WatermarkComboBox is the only control that does not use the Background property but uses WatermarkBackground property instead.
                    if (AssociatedObject is WatermarkComboBox watermarkCombo) {
                        watermarkCombo.SetValue(WatermarkComboBox.WatermarkBackgroundProperty, background);
                    }
                }
            }

            private void ClearBackground()
            {
                AssociatedObject.ClearValue(Control.BackgroundProperty);
                if (AssociatedObject is WatermarkComboBox watermarkCombo) {
                    watermarkCombo.ClearValue(WatermarkComboBox.WatermarkBackgroundProperty);
                }
            }
        }
    }
}
