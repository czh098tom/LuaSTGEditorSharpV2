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
    public class InsertAsChildFactory(EditorNodeFactory editorNodeFactory) : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend)
        {
            return new AddChildCommand(editorNodeFactory, origin, origin.PhysicalChildren.Count, toAppend);
        }
    }
}
