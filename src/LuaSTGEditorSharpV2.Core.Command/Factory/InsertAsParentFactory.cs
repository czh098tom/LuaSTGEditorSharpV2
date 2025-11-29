using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    [Inject(ServiceLifetime.Singleton)]
    public class InsertAsParentFactory() : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(EditorNode origin, IReadOnlyList<NodeData> toAppend)
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
                        var cmd = CheckedCommand.InsertNodeAsParent(origin, n);
                        yield return cmd;
                        if (cmd == null) yield break;
                    }
                    else
                    {
                        if (originParent == null || originIndex < 0)
                        {
                            yield break;
                        }
                        yield return CheckedCommand.InsertNodeAfter(originParent.Children[originIndex + idx - 1], n);
                    }
                    idx++;
                }
            }
            return Commands.FromEnumerable(Get());
        }
    }
}
