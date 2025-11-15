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
    public class InsertAsParentFactory(EditorNodeFactory editorNodeFactory) : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend)
        {
            return new CompositeCommand(CreateCommands(origin, toAppend));
        }

        private IEnumerable<CommandBase> CreateCommands(NodeData origin, NodeData toAppend)
        {
            var parent = origin.PhysicalParent;
            if (parent == null) yield break;
            var idx = parent.PhysicalChildren.FindIndex(origin);
            if (idx == -1) yield break;
            yield return new RemoveChildCommand(editorNodeFactory, parent, idx);
            yield return new AddChildCommand(editorNodeFactory, parent, idx, toAppend);
            var target = parent.PhysicalChildren[idx];
            yield return new AddChildCommand(editorNodeFactory, target, toAppend.PhysicalChildren.Count, origin);
        }
    }
}
