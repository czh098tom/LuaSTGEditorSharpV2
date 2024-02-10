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
        public static CommandBase? SelectCommand(this IEnumerable<NodeData> nodes, 
            Func<NodeData, CommandBase?> commandGenerator)
        {
            List<CommandBase> commands = [];
            foreach (var n in nodes)
            {
                var command = commandGenerator(n);
                if (command != null)
                {
                    commands.Add(command);
                }
            }
            if (commands.Count > 0)
            {
                return new CompositeCommand(commands);
            }
            return null;
        }
    }
}
