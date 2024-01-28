using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    public interface IInsertCommandFactory
    {
        CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend);
    }
}
