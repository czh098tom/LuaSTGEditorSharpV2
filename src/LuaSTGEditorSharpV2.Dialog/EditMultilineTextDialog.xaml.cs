using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Dialog;

public partial class EditMultilineTextDialog : OKCancelWindow
{
    public EditMultilineTextDialog()
    {
        InitializeComponent();
    }

    public EditMultilineTextDialog(string text)
        : this()
    {
        textEditor.Text = text;
    }

    public string Text => textEditor.Text;
}
