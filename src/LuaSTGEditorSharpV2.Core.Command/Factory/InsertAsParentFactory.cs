using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    public class InsertAsParentFactory : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend)
        {
            if (origin.PhysicalParent == null) return null;
            var idx = origin.PhysicalParent.PhysicalChildren.FindIndex(origin);
            if (idx == -1) return null;
            var removeCurr = new RemoveChildCommand(origin.PhysicalParent, idx);
            var addToAppend = new AddChildCommand(origin.PhysicalParent, idx, toAppend);
            var addOriginal = new AddChildCommand(toAppend, toAppend.PhysicalChildren.Count, origin);
            return new CompositeCommand(removeCurr, addToAppend, addOriginal);
        }
    }
}
