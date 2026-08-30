using System.Windows;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Dialog;
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Services;

[Inject(ServiceLifetime.Singleton, typeof(ICodeEditDialogService))]
public sealed class CodeEditDialogService : ICodeEditDialogService
{
    public string? EditCode(string title, string initialValue)
    {
        var dialog = new EditCodeDialog(initialValue)
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
