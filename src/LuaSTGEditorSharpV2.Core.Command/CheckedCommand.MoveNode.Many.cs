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
                public static CommandBase? ToBefore(EditorNode origin, IEnumerable<EditorNode> toMove)
                {
                    return toMove.SelectFilter(n => MoveNode.ToBefore(origin, n));
                }
                public static CommandBase? ToAfter(EditorNode origin, IEnumerable<EditorNode> toMove)
                {
                    return toMove.Reverse().SelectFilter(n => MoveNode.ToAfter(origin, n));
                }
                public static CommandBase? AsLastChild(EditorNode origin, IEnumerable<EditorNode> toMove)
                {
                    return toMove.SelectFilter(n => MoveNode.AsLastChild(origin, n));
                }
                public static CommandBase? AsFirstChild(EditorNode origin, IEnumerable<EditorNode> toMove)
                {
                    return toMove.Reverse().SelectFilter(n => MoveNode.AsFirstChild(origin, n));
                }
            }
        }
    }
}
