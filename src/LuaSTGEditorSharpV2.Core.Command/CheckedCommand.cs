using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static partial class CheckedCommand
    {
        public static CommandBase? RemoveNode(EditorDocument doc, NodePath p)
        {
            var n = doc.RootEditorNode.GetNodeByPath(p);
            if (n?.Parent == null) throw new CommandExecutionException();
            return AtomicCommand.RemoveNode(doc, n.Parent.GetPath(), n.Parent.Children.FindIndex(n));
        }
    }
}
