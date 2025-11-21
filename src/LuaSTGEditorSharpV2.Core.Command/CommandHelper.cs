using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static class CommandHelper
    {
        public static CommandBase? SelectFilter<T>(this IEnumerable<T> nodes, 
            Func<T, CommandBase?> commandGenerator)
        {
            return new CompositeCommand(nodes.Select(commandGenerator).OfType<CommandBase>());
        }

        public static CommandBase? SelectFilter<T>(this IReadOnlyCollection<T> nodes,
            Func<T, CommandBase?> commandGenerator)
        {
            return FromList([.. nodes.Select(commandGenerator).OfType<CommandBase>()]);
        }

        public static CommandBase? FromList(IReadOnlyList<CommandBase>? commands)
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
    }
}
