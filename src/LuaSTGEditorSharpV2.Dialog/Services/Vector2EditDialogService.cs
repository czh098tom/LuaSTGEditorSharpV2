using System.Linq;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Dialog;

namespace LuaSTGEditorSharpV2.Dialog.Services;

public record struct Vector2EditResult(string X, string Y);

[Inject(ServiceLifetime.Singleton)]
public sealed class Vector2EditDialogService
{
    public Vector2EditResult? EditVector2(string title, string initialX, string initialY)
    {
        var dialog = new Vector2EditDialog(initialX, initialY)
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
        return dialog.ShowDialog() == true
            ? new Vector2EditResult(dialog.X, dialog.Y)
            : null;
    }
}
