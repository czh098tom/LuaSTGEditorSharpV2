using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LuaSTGEditorSharpV2.WPF
{
    public static class WindowHelper
    {
        public static Window? GetMainWindow()
        {
            var windows = new ArrayList(Application.Current.Windows);
            foreach (var win in windows)
            {
                if (win is IMainWindow window)
                {
                    return window as Window;
                }
            }
            return null;
        }
    }
}
