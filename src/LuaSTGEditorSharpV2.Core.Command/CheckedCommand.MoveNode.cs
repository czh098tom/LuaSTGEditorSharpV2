using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static partial class CheckedCommand
    {
        public static partial class MoveNode
        {
            public static CommandBase? ToBefore(EditorDocument document, NodePath path, EditorNode toMove)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path);
                if (origin?.Parent == null || toMove.Parent == null) throw new CommandExecutionException();
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent;
                    var originParent = origin.Parent;
                    if (originParent == null || toMoveParent == null) throw new CommandExecutionException();
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) throw new CommandExecutionException();
                    yield return AtomicCommand.RemoveNode(document, toMoveParent.GetPath(), toMoveIdx);
                    var originIdx = originParent.Children.FindIndex(origin);
                    yield return AtomicCommand.AddNode(document, originParent.GetPath(), originIdx, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }

            public static CommandBase? ToAfter(EditorDocument document, NodePath path, EditorNode toMove)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path);
                if (origin?.Parent == null || toMove.Parent == null) throw new CommandExecutionException();
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent;
                    var originParent = origin.Parent;
                    if (originParent == null || toMoveParent == null) throw new CommandExecutionException();
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) throw new CommandExecutionException();
                    yield return AtomicCommand.RemoveNode(document, toMoveParent.GetPath(), toMoveIdx);
                    var originIdx = originParent.Children.FindIndex(origin);
                    yield return AtomicCommand.AddNode(document, originParent.GetPath(), originIdx + 1, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }

            public static CommandBase? AsLastChild(EditorDocument document, NodePath path, EditorNode toMove)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path);
                if (origin == null || toMove.Parent == null) throw new CommandExecutionException();
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent ?? throw new CommandExecutionException();
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) throw new CommandExecutionException();
                    yield return AtomicCommand.RemoveNode(document, toMoveParent.GetPath(), toMoveIdx);
                    yield return AtomicCommand.AddNode(document, path, origin.Children.Count, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }

            public static CommandBase? AsFirstChild(EditorDocument document, NodePath path, EditorNode toMove)
            {
                var origin = document.RootEditorNode.GetNodeByPath(path);
                if (origin == null || toMove.Parent == null) throw new CommandExecutionException();
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent ?? throw new CommandExecutionException();
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) throw new CommandExecutionException();
                    yield return AtomicCommand.RemoveNode(document, toMoveParent.GetPath(), toMoveIdx);
                    yield return AtomicCommand.AddNode(document, path, 0, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }
        }
    }
}
