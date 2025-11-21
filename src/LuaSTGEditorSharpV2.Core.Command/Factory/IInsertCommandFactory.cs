using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    public interface IInsertCommandFactory
    {
        CommandBase? CreateInsertCommand(EditorNode origin, NodeData toAppend);
    }
}
