using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LuaSTGEditorSharpV2.RoutedCommands
{
    public static class LuaSTGBuildingCommand
    {
        public static readonly RoutedUICommand BuildSelected;

        static LuaSTGBuildingCommand()
        {
            BuildSelected = new RoutedUICommand(
                "Build Selected",
                "BuildSelected",
                typeof(LuaSTGBuildingCommand));
        }
    }
}
