using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using LuaSTGEditorSharpV2.Dialog.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Dialog;

public partial class Vector2EditDialog : OKCancelWindow
{
    private TextBox? _activeTextBox;

    public Vector2EditDialogViewModel ViewModel
        => (DataContext as Vector2EditDialogViewModel) ?? throw new InvalidCastException();

    public Vector2EditDialog()
    {
        InitializeComponent();
        AddHandler(
            Keyboard.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(TextBox_GotKeyboardFocus),
            handledEventsToo: true);
        AddHandler(
            TextBoxBase.SelectionChangedEvent,
            new RoutedEventHandler(TextBox_SelectionChanged),
            handledEventsToo: true);
        ViewModel.TextInserted += OnTextInserted;
        ViewModel.VectorEditorAppended += OnVectorEditorAppended;
    }

    public Vector2EditDialog(string x, string y)
        : this()
    {
        ViewModel.SetInitialValues(x, y);
        expressionTextBox.Focus();
        expressionTextBox.CaretIndex = expressionTextBox.Text.Length;
    }

    public string Expression => ViewModel.Expression;
    public string X => ViewModel.X;
    public string Y => ViewModel.Y;

    private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        => ReportTextBoxSelection(e.OriginalSource as TextBox);

    private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        => ReportTextBoxSelection(e.OriginalSource as TextBox);

    private void ReportTextBoxSelection(TextBox? textBox)
    {
        if (textBox is null || GetTextBoxTarget(textBox) is not { } target)
        {
            return;
        }

        _activeTextBox = textBox;
        ViewModel.SetTextSelection(target, textBox.SelectionStart, textBox.SelectionLength);
    }

    private void OnTextInserted(int caretIndex)
    {
        if (_activeTextBox is null)
        {
            return;
        }

        _activeTextBox.Focus();
        _activeTextBox.CaretIndex = caretIndex;
    }

    private void OnVectorEditorAppended()
    {
        expressionTextBox.Focus();
        expressionTextBox.CaretIndex = expressionTextBox.Text.Length;
    }

    private static Vector2EditTextBoxTarget? GetTextBoxTarget(TextBox textBox)
    {
        for (DependencyObject? current = textBox; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is FrameworkElement { Tag: string tag }
                && Enum.TryParse<Vector2EditTextBoxTarget>(tag, out var target))
            {
                return target;
            }
        }

        return null;
    }
}
