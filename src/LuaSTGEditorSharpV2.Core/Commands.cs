using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public static class Commands
    {
        public static CommandBase? FromFilteredList(IReadOnlyList<CommandBase>? commands)
        {
            if (commands == null)
            {
                return null;
            }
            var commandList = commands;
            if (commandList.Count == 0)
            {
                return null;
            }
            else if (commandList.Count == 1)
            {
                return commandList[0];
            }
            else
            {
                return new CompositeCommand(commandList);
            }
        }

        public static CommandBase? FromFilteredEnumerable(IEnumerable<CommandBase>? commands)
        {
            if (commands == null)
            {
                return null;
            }
            return new CompositeCommand(commands);
        }

        public static CommandBase? FromEnumerable(IEnumerable<CommandBase?>? commands)
        {
            return FromFilteredEnumerable(commands?.OfType<CommandBase>());
        }
    }
}
