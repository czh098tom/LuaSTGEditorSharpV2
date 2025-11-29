using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static class CheckedCommand
    {
        public static CommandBase? ModifyProperty(EditorNode node, string? propertyName, string newValue)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;
            if (node.Source.HasProperty(propertyName))
            {
                return AtomicCommand.EditProperty(node, propertyName, newValue);
            }
            else
            {
                return AtomicCommand.AddProperty(node, propertyName, newValue);
            }
        }

        public static CommandBase? InsertNodeBefore(EditorNode origin, NodeData toAppend)
        {
            if (origin.Parent == null) return null;
            int idx = origin.Parent.Children.FindIndex(origin);
            if (idx < 0) return null;
            return AtomicCommand.AddNode(origin.Parent, idx, toAppend);
        }

        public static CommandBase? InsertNodeAfter(EditorNode origin, NodeData toAppend)
        {
            if (origin.Parent == null) return null;
            int idx = origin.Parent.Children.FindIndex(origin);
            if (idx < 0) return null;
            return AtomicCommand.AddNode(origin.Parent, idx + 1, toAppend);
        }

        public static CommandBase? InsertNodeAsLastChild(EditorNode origin, NodeData toAppend)
        {
            return AtomicCommand.AddNode(origin, origin.Children.Count, toAppend);
        }

        public static CommandBase? InsertNodeAsFirstChild(EditorNode origin, NodeData toAppend)
        {
            return AtomicCommand.AddNode(origin, 0, toAppend);
        }

        public static CommandBase? InsertNodeAsParent(EditorNode origin, NodeData toAppend)
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

        public static CommandBase? RemoveNode(EditorNode n)
        {
            if (n.Parent == null) return null;
            return AtomicCommand.RemoveNode(n.Parent, n.Parent.Children.FindIndex(n));
        }

        public static CommandBase? MoveToBefore(EditorNode origin, EditorNode toMove)
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

        public static CommandBase? MoveToAfter(EditorNode origin, EditorNode toMove)
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

        public static CommandBase? MoveAsLastChild(EditorNode origin, EditorNode toMove)
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

        public static CommandBase? MoveAsFirstChild(EditorNode origin, EditorNode toMove)
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
