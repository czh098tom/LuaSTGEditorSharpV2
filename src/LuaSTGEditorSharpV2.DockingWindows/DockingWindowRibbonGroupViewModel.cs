using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    public class DockingWindowRibbonGroupViewModel
    {
        public event EventHandler<Type>? OnShift;

        public void RaiseOnShift(object? sender, Type type)
        {
            OnShift?.Invoke(sender, type);
        }
    }
}
