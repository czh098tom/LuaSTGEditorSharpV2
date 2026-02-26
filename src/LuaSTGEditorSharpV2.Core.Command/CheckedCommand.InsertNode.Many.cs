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
                public static CommandBase? ToBefore(EditorDocument document, NodePath path, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.SelectFilter(n => InsertNode.ToBefore(document, path, n));
                }

                public static CommandBase? ToAfter(EditorDocument document, NodePath path, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.Reverse().SelectFilter(n => InsertNode.ToAfter(document, path, n));
                }

                public static CommandBase? AsLastChild(EditorDocument document, NodePath path, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.SelectFilter(n => InsertNode.AsLastChild(document, path, n));
                }

                public static CommandBase? AsFirstChild(EditorDocument document, NodePath path, IEnumerable<NodeData> toAppend)
                {
                    return toAppend.Reverse().SelectFilter(n => InsertNode.AsFirstChild(document, path, n));
                }

                public static CommandBase? AsParent(EditorDocument document, NodePath path, IEnumerable<NodeData> toAppend)
                {
                    var origin = document.RootEditorNode.GetNodeByPath(path) ?? throw new CommandExecutionException();
                    IEnumerable<CommandBase?> Get()
                    {
                        var originParent = origin.Parent;
                        var originIndex = originParent?.Children.FindIndex(origin) ?? -1;
                        var idx = 0;
                        foreach (var n in toAppend)
                        {
                            if (idx == 0)
                            {
                                var cmd = InsertNode.AsParent(document, path, n);
                                yield return cmd;
                            }
                            else
                            {
                                if (originParent == null || originIndex < 0)
                                {
                                    throw new CommandExecutionException();
                                }
                                yield return InsertNode.ToAfter(document, originParent.Children[originIndex + idx - 1].GetPath(), n);
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
