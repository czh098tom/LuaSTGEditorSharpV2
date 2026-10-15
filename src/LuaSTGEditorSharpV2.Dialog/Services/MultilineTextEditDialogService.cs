using System.Linq;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Dialog;

namespace LuaSTGEditorSharpV2.Dialog.Services;

[Inject(ServiceLifetime.Singleton)]
public sealed class MultilineTextEditDialogService
{
    public string? EditText(string title, string initialValue)
    {
        var dialog = new EditMultilineTextDialog(initialValue)
        {
            Title = title,
        };
        var owner = Application.Current?.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive);
        if (owner is not null)
        {
            dialog.Owner = owner;
        }
        return dialog.ShowDialog() == true ? dialog.Text : null;
    }
}
