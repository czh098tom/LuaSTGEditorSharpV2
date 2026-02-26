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
            public static class Many
            {
                public static CommandBase? ToBefore(EditorDocument document, NodePath path, IEnumerable<EditorNode> toMove)
                {
                    return toMove.SelectFilter(n => MoveNode.ToBefore(document, path, n));
                }
                public static CommandBase? ToAfter(EditorDocument document, NodePath path, IEnumerable<EditorNode> toMove)
                {
                    return toMove.Reverse().SelectFilter(n => MoveNode.ToAfter(document, path, n));
                }
                public static CommandBase? AsLastChild(EditorDocument document, NodePath path, IEnumerable<EditorNode> toMove)
                {
                    return toMove.SelectFilter(n => MoveNode.AsLastChild(document, path, n));
                }
                public static CommandBase? AsFirstChild(EditorDocument document, NodePath path, IEnumerable<EditorNode> toMove)
                {
                    return toMove.Reverse().SelectFilter(n => MoveNode.AsFirstChild(document, path, n));
                }
            }
        }
    }
}
