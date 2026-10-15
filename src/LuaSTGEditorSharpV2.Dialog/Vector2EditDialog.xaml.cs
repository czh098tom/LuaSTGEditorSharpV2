using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LuaSTGEditorSharpV2.Dialog.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Dialog;

public partial class Vector2EditDialog : OKCancelWindow
{
    public static readonly RoutedUICommand InsertTextCommand = new(
        "Insert text",
        nameof(InsertTextCommand),
        typeof(Vector2EditDialog));

    private TextBox? _activeTextBox;

    public Vector2EditDialogViewModel ViewModel
        => DataContext as Vector2EditDialogViewModel
           ?? throw new InvalidCastException();

    public Vector2EditDialog()
    {
        InitializeComponent();
        CommandBindings.Add(new CommandBinding(
            InsertTextCommand,
            InsertTextCommand_Executed,
            InsertTextCommand_CanExecute));
        AddHandler(
            Keyboard.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(
                TextBox_GotKeyboardFocus),
            handledEventsToo: true);
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

    private void TextBox_GotKeyboardFocus(
        object sender,
        KeyboardFocusChangedEventArgs e)
    {
        if (e.OriginalSource is TextBox textBox)
        {
            _activeTextBox = textBox;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void InsertTextCommand_CanExecute(
        object sender,
        CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = e.Parameter is string { Length: > 0 }
            && _activeTextBox is { } textBox
            && ContainsVisual(textBox);
        e.Handled = true;
    }

    private void InsertTextCommand_Executed(
        object sender,
        ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not string { Length: > 0 } insertion
            || _activeTextBox is not { } textBox
            || !ContainsVisual(textBox))
        {
            return;
        }

        var caretIndex = textBox.SelectionStart + insertion.Length;
        textBox.SelectedText = insertion;
        textBox.Select(caretIndex, 0);
        textBox.Focus();
        e.Handled = true;
    }

    private bool ContainsVisual(DependencyObject visual)
    {
        for (var current = visual;
             current is not null;
             current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, this))
            {
                return true;
            }
        }

        return false;
    }

    private void OnVectorEditorAppended()
    {
        expressionTextBox.Focus();
        expressionTextBox.CaretIndex = expressionTextBox.Text.Length;
    }
}
