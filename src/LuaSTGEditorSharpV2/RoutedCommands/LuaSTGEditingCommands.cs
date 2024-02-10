using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LuaSTGEditorSharpV2.RoutedCommands
{
    public static class LuaSTGEditingCommands
    {
        public static readonly RoutedUICommand ViewCode;
        public static readonly RoutedUICommand ExportCode;

        static LuaSTGEditingCommands()
        {
            ViewCode = new RoutedUICommand(
                "View Code",
                "ViewCode",
                typeof(LuaSTGEditingCommands));
            ExportCode = new RoutedUICommand(
                "Export Code",
                "ExportCode",
                typeof(LuaSTGEditingCommands));
        }
    }
}
