using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command.Factory
{
    [Inject(ServiceLifetime.Singleton)]
    public class InsertAsParentFactory(ViewModelProviderServiceProvider viewModelProviderService) : IInsertCommandFactory
    {
        public CommandBase? CreateInsertCommand(NodeData origin, NodeData toAppend)
        {
            if (origin.PhysicalParent == null) return null;
            var idx = origin.PhysicalParent.PhysicalChildren.FindIndex(origin);
            if (idx == -1) return null;
            var removeCurr = new RemoveChildCommand(viewModelProviderService, origin.PhysicalParent, idx);
            var addToAppend = new AddChildCommand(viewModelProviderService, origin.PhysicalParent, idx, toAppend);
            var addOriginal = new AddChildCommand(viewModelProviderService, toAppend, toAppend.PhysicalChildren.Count, origin);
            return new CompositeCommand(removeCurr, addToAppend, addOriginal);
        }
    }
}
