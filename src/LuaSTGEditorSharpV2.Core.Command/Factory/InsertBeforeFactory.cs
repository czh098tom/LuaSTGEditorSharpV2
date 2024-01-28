using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    public class InsertBeforeFactory : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend)
        {
            if (origin.PhysicalParent == null) return null;
            int idx = origin.PhysicalParent.PhysicalChildren.FindIndex(origin);
            if (idx < 0) return null;
            return new AddChildCommand(origin.PhysicalParent, idx, toAppend);
        }
    }
}
