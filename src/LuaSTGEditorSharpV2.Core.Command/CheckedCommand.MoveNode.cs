using LuaSTGEditorSharpV2.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static partial class CheckedCommand
    {
        public static partial class MoveNode
        {
            public static CommandBase? ToBefore(EditorNode origin, EditorNode toMove)
            {
                if (origin.Parent == null || toMove.Parent == null) return null;
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent;
                    var originParent = origin.Parent;
                    if (originParent == null || toMoveParent == null) yield break;
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) yield break;
                    yield return AtomicCommand.RemoveNode(toMoveParent, toMoveIdx);
                    var originIdx = originParent.Children.FindIndex(origin);
                    yield return AtomicCommand.AddNode(originParent, originIdx, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }

            public static CommandBase? ToAfter(EditorNode origin, EditorNode toMove)
            {
                if (origin.Parent == null || toMove.Parent == null) return null;
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent;
                    var originParent = origin.Parent;
                    if (originParent == null || toMoveParent == null) yield break;
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) yield break;
                    yield return AtomicCommand.RemoveNode(toMoveParent, toMoveIdx);
                    var originIdx = originParent.Children.FindIndex(origin);
                    yield return AtomicCommand.AddNode(originParent, originIdx + 1, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }

            public static CommandBase? AsLastChild(EditorNode origin, EditorNode toMove)
            {
                if (toMove.Parent == null) return null;
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent;
                    if (toMoveParent == null) yield break;
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) yield break;
                    yield return AtomicCommand.RemoveNode(toMoveParent, toMoveIdx);
                    yield return AtomicCommand.AddNode(origin, origin.Children.Count, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }

            public static CommandBase? AsFirstChild(EditorNode origin, EditorNode toMove)
            {
                if (toMove.Parent == null) return null;
                IEnumerable<CommandBase> Get()
                {
                    var toMoveParent = toMove.Parent;
                    if (toMoveParent == null) yield break;
                    var toMoveIdx = toMoveParent.Children.FindIndex(toMove);
                    var toMoveSource = toMove.Source;
                    if (toMoveIdx < 0) yield break;
                    yield return AtomicCommand.RemoveNode(toMoveParent, toMoveIdx);
                    yield return AtomicCommand.AddNode(origin, 0, toMoveSource);
                }
                return Commands.FromEnumerable(Get());
            }
        }
    }
}
