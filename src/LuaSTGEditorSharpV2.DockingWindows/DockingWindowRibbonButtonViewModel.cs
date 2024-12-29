using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    public class DockingWindowRibbonButtonViewModel
    {
        public ICommand Command { get; set; }

        public event EventHandler<Type>? OnShift;

        public DockingWindowRibbonButtonViewModel(Type t) 
        {
            Command = new RelayCommand(() =>
            {
                OnShift?.Invoke(this, t);
            });
        }
    }
}
