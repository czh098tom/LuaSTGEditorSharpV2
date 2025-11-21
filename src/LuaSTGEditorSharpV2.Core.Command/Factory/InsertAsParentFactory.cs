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
        public CommandBase? CreateInsertCommand(EditorNode origin, NodeData toAppend)
        {
            return new CompositeCommand(CreateCommands(origin, toAppend));
        }

        private static IEnumerable<CommandBase> CreateCommands(EditorNode origin, NodeData toAppend)
        {
            var parent = origin.Parent;
            if (parent == null) yield break;
            var idx = parent.Source.PhysicalChildren.FindIndex(origin.Source);
            if (idx == -1) yield break;
            var originSource = origin.Source;
            yield return AtomicCommand.RemoveNode(parent, idx);
            yield return AtomicCommand.AddNode(parent, idx, toAppend);
            var target = parent.Children[idx];
            yield return AtomicCommand.AddNode(target, toAppend.PhysicalChildren.Count, originSource);
        }
    }
}
