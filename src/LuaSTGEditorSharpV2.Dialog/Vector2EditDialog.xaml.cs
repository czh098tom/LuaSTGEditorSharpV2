using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using LuaSTGEditorSharpV2.Dialog.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Dialog;

public partial class Vector2EditDialog : OKCancelWindow
{
    private TextBox? _previousTextBox;
    private int _previousSelectionStart;
    private int _previousSelectionLength;

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
        => RememberTextBox(e.OriginalSource as TextBox);

    private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        => RememberTextBox(e.OriginalSource as TextBox);

    private void RememberTextBox(TextBox? textBox)
    {
        if (textBox is null)
        {
            return;
        }

        _previousTextBox = textBox;
        _previousSelectionStart = textBox.SelectionStart;
        _previousSelectionLength = textBox.SelectionLength;
    }

    private void InsertButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string insertion)
        {
            return;
        }

        var textBox = _previousTextBox ?? expressionTextBox;
        var start = Math.Clamp(_previousSelectionStart, 0, textBox.Text.Length);
        var length = Math.Clamp(_previousSelectionLength, 0, textBox.Text.Length - start);
        textBox.Select(start, length);
        textBox.SelectedText = insertion;
        textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        textBox.Focus();
        textBox.CaretIndex = start + insertion.Length;
        RememberTextBox(textBox);
    }

    private void OpenVectorEditor_Click(object sender, RoutedEventArgs e)
    {
        var editor = new VectorEditorDialog
        {
            Owner = this,
        };
        if (editor.ShowDialog() == true)
        {
            ViewModel.AppendVector(editor.OffsetX, editor.OffsetY);
            expressionTextBox.Focus();
            expressionTextBox.CaretIndex = expressionTextBox.Text.Length;
        }
    }
}
