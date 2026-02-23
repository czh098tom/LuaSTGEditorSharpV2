using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LuaSTGEditorSharpV2.WPF
{
    public class LoggingTextBox : TextBox
    {
        public static readonly DependencyProperty LastAppendedTextProperty =
            DependencyProperty.Register(
                nameof(LastAppendedText),
                typeof(string),
                typeof(LoggingTextBox),
                new PropertyMetadata(LastAppendedTextChangedEventHandler));

        public static readonly DependencyProperty ClearStreamProperty =
            DependencyProperty.Register(
                nameof(ClearStream),
                typeof(EventArgs),
                typeof(LoggingTextBox),
                new PropertyMetadata(ClearStreamChangedEventHandler));

        public string? LastAppendedText 
        { 
            get => (string?)GetValue(LastAppendedTextProperty); 
            set => SetValue(LastAppendedTextProperty, value);
        }

        public EventArgs? ClearStream
        {
            get => (EventArgs?)GetValue(ClearStreamProperty);
            set => SetValue(ClearStreamProperty, value);
        }

        private static void LastAppendedTextChangedEventHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoggingTextBox ltb && e.NewValue is string text)
            {
                ltb.AppendText(text);
                ltb.ScrollToEnd();
            }
        }

        private static void ClearStreamChangedEventHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoggingTextBox ltb)
            {
                ltb.Clear();
            }
        }

        public LoggingTextBox() : base()
        {
            this.IsReadOnly = true;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.TextWrapping = TextWrapping.Wrap;
        }
    }
}
