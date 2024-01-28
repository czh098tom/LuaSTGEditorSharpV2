using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    public class InsertAfterFactory : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend)
        {
            if (origin.PhysicalParent == null) return null;
            int idx = origin.PhysicalParent.PhysicalChildren.FindIndex(origin);
            if (idx < 0) return null;
            return new AddChildCommand(origin.PhysicalParent, idx + 1, toAppend);
        }
    }
}
