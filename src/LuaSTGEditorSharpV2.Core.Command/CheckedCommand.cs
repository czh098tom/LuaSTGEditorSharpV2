using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static partial class CheckedCommand
    {
        public static CommandBase? RemoveNode(EditorNode n)
        {
            if (n.Parent == null) return null;
            return AtomicCommand.RemoveNode(n.Parent, n.Parent.Children.FindIndex(n));
        }
    }
}
