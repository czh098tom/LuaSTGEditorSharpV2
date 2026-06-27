using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    [Inject(ServiceLifetime.Singleton)]
    public class InsertBeforeFactory() : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(EditorDocument document, NodePath path, IReadOnlyList<NodeData> toAppend)
        {
            return CheckedCommand.InsertNode.Many.ToBefore(document, path, toAppend);
        }
    }
}
