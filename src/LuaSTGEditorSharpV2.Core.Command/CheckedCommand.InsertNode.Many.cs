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
            public static class Many
            {
                public static CommandBase? ToBefore(EditorNode origin, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.SelectFilter(n => InsertNode.ToBefore(origin, n));
                }

                public static CommandBase? ToAfter(EditorNode origin, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.Reverse().SelectFilter(n => InsertNode.ToAfter(origin, n));
                }

                public static CommandBase? AsLastChild(EditorNode origin, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.SelectFilter(n => InsertNode.AsLastChild(origin, n));
                }

                public static CommandBase? AsFirstChild(EditorNode origin, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.Reverse().SelectFilter(n => InsertNode.AsFirstChild(origin, n));
                }

                public static CommandBase? AsParent(EditorNode origin, IEnumerable<NodeData> toAppend)
                {
                    IEnumerable<CommandBase?> Get()
                    {
                        var originParent = origin.Parent;
                        var originIndex = originParent?.Children.FindIndex(origin) ?? -1;
                        var idx = 0;
                        foreach (var n in toAppend)
                        {
                            if (idx == 0)
                            {
                                var cmd = InsertNode.AsParent(origin, n);
                                yield return cmd;
                                if (cmd == null) yield break;
                            }
                            else
                            {
                                if (originParent == null || originIndex < 0)
                                {
                                    yield break;
                                }
                                yield return InsertNode.ToAfter(originParent.Children[originIndex + idx - 1], n);
                            }
                            idx++;
                        }
                    }
                    return Commands.FromEnumerable(Get());
                }
            }
        }
    }
}
