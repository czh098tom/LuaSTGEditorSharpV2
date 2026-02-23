using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LuaSTGEditorSharpV2.RoutedCommands
{
    public static class LuaSTGExecutionCommand
    {
        public static readonly RoutedUICommand ExecuteSelected;

        static LuaSTGExecutionCommand()
        {
            ExecuteSelected = new RoutedUICommand(
                "Execute Selected",
                "ExecuteSelected",
                typeof(LuaSTGExecutionCommand));
        }
    }
}
