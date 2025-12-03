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
        public static partial class InsertNode
        {
            public static CommandBase? ToBefore(EditorNode origin, NodeData toAppend)
            {
                if (origin.Parent == null) return null;
                int idx = origin.Parent.Children.FindIndex(origin);
                if (idx < 0) return null;
                return AtomicCommand.AddNode(origin.Parent, idx, toAppend);
            }

            public static CommandBase? ToAfter(EditorNode origin, NodeData toAppend)
            {
                if (origin.Parent == null) return null;
                int idx = origin.Parent.Children.FindIndex(origin);
                if (idx < 0) return null;
                return AtomicCommand.AddNode(origin.Parent, idx + 1, toAppend);
            }

            public static CommandBase? AsLastChild(EditorNode origin, NodeData toAppend)
            {
                return AtomicCommand.AddNode(origin, origin.Children.Count, toAppend);
            }

            public static CommandBase? AsFirstChild(EditorNode origin, NodeData toAppend)
            {
                return AtomicCommand.AddNode(origin, 0, toAppend);
            }

            public static CommandBase? AsParent(EditorNode origin, NodeData toAppend)
            {
                IEnumerable<CommandBase> Get()
                {
                    var parent = origin.Parent;
                    if (parent == null) yield break;
                    var idx = parent.Children.FindIndex(origin);
                    if (idx == -1) yield break;
                    var originSource = origin.Source;
                    yield return AtomicCommand.RemoveNode(parent, idx);
                    yield return AtomicCommand.AddNode(parent, idx, toAppend);
                    var target = parent.Children[idx];
                    yield return AtomicCommand.AddNode(target, toAppend.PhysicalChildren.Count, originSource);
                }
                return Commands.FromFilteredEnumerable(Get());
            }
        }
    }
}
