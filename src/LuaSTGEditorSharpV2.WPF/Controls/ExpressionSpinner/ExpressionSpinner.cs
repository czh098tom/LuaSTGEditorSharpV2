using System.Windows;
using System.Windows.Controls;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using Xceed.Wpf.Toolkit;

namespace LuaSTGEditorSharpV2.WPF.Controls
{
    /// <summary>
    /// Spinner for numeric or additive-expression text input.
    /// </summary>
    public class ExpressionSpinner : ButtonSpinner
    {
        static ExpressionSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionSpinner),
                new FrameworkPropertyMetadata(typeof(ExpressionSpinner)));
        }

        #region Dependency Properties

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(decimal), typeof(ExpressionSpinner),
                new PropertyMetadata(1.0m));

        public decimal Step
        {
            get => (decimal)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(decimal?), typeof(ExpressionSpinner),
            new PropertyMetadata(null));

        public decimal? Maximum
        {
            get => (decimal?)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

		public static readonly DependencyProperty MinimumProperty =
			DependencyProperty.Register(nameof(Minimum), typeof(decimal?), typeof(ExpressionSpinner),
			new PropertyMetadata(null));

		public decimal? Minimum
		{
			get => (decimal?)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
		}

		public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(nameof(Watermark), typeof(object), typeof(ExpressionSpinner),
                new PropertyMetadata(null));

        public object? Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ExpressionSpinner),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ExpressionSpinner spinner)
                return;
            spinner.AllowSpin = !string.IsNullOrEmpty(spinner.Text) || decimal.TryParse(spinner.Text, out _);
        }

        protected override void OnSpin(SpinEventArgs e)
        {
            if (!AllowSpin)
                return;

            base.OnSpin(e);
            var step = e.Direction == SpinDirection.Increase ? Step : -Step;
            SetCurrentValue(TextProperty, ScalarExpressionCodec.Spin(Text, step, Minimum, Maximum));
        }
    }
}
