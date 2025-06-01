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
    public class InsertBeforeFactory(EditorNodeFactory editorNodeFactory) : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend)
        {
            if (origin.PhysicalParent == null) return null;
            int idx = origin.PhysicalParent.PhysicalChildren.FindIndex(origin);
            if (idx < 0) return null;
            return new AddChildCommand(editorNodeFactory, origin.PhysicalParent, idx, toAppend);
        }
    }
}
