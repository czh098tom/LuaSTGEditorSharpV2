using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public static class CompositeCommandExtension
    {
        public static CommandBase? SelectFilter<T>(this IEnumerable<T> nodes,
            Func<T, CommandBase?> commandGenerator)
        {
            return Commands.FromFilteredEnumerable(nodes.Select(commandGenerator).OfType<CommandBase>());
        }
    }
}
