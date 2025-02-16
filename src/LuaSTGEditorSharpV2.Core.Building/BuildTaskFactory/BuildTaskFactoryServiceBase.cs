using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory
{
    public class BuildTaskFactoryServiceBase(BuildTaskFactoryServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
        : ProviderCachedNodeServiceBase<BuildTaskFactoryServiceProvider>(nodeServiceProvider, serviceProvider)
    {
        public virtual WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, 
            BuildTaskFactoryContext context)
        {
            return NodeServiceProvider.ProceedChildren(nodeData, context);
        }
    }
}
