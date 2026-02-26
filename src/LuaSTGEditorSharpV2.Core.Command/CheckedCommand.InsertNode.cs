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
            public static CommandBase? ToBefore(EditorDocument document, NodePath path, NodeData toAppend)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path);
                if (origin?.Parent == null) throw new CommandExecutionException();
                int idx = origin.Parent.Children.FindIndex(origin);
                if (idx < 0) throw new CommandExecutionException();
                return AtomicCommand.AddNode(document, origin.Parent.GetPath(), idx, toAppend);
            }

            public static CommandBase? ToAfter(EditorDocument document, NodePath path, NodeData toAppend)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path);
                if (origin?.Parent == null) throw new CommandExecutionException();
                int idx = origin.Parent.Children.FindIndex(origin);
                if (idx < 0) throw new CommandExecutionException();
                return AtomicCommand.AddNode(document, origin.Parent.GetPath(), idx + 1, toAppend);
            }

            public static CommandBase? AsLastChild(EditorDocument document, NodePath path, NodeData toAppend)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path) ?? throw new CommandExecutionException();
                return AtomicCommand.AddNode(document, path, origin.Children.Count, toAppend);
            }

            public static CommandBase? AsFirstChild(EditorDocument document, NodePath path, NodeData toAppend)
            {
                return AtomicCommand.AddNode(document, path, 0, toAppend);
            }

            public static CommandBase? AsParent(EditorDocument document, NodePath path, NodeData toAppend)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path) ?? throw new CommandExecutionException();
                IEnumerable<CommandBase> Get()
                {
                    var parent = origin.Parent ?? throw new CommandExecutionException();
                    var idx = parent.Children.FindIndex(origin);
                    if (idx == -1) throw new CommandExecutionException();
                    var originSource = origin.Source;
                    var parentPath = parent.GetPath();
                    yield return AtomicCommand.RemoveNode(document, parentPath, idx);
                    yield return AtomicCommand.AddNode(document, parentPath, idx, toAppend);
                    var target = parent.Children[idx];
                    yield return AtomicCommand.AddNode(document, target.GetPath(), toAppend.PhysicalChildren.Count, originSource);
                }
                return Commands.FromFilteredEnumerable(Get());
            }
        }
    }
}
