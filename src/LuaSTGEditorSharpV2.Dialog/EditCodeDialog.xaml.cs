using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Dialog;

public partial class EditCodeDialog : OKCancelWindow
{
    public EditCodeDialog()
    {
        InitializeComponent();
    }

    public EditCodeDialog(string text)
        : this()
    {
        textEditor.Text = text;
    }

    public string Text => textEditor.Text;
}
