using System.Linq;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Dialog;
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Services;

[Inject(ServiceLifetime.Singleton, typeof(IMultilineTextEditDialogService))]
public sealed class MultilineTextEditDialogService : IMultilineTextEditDialogService
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
