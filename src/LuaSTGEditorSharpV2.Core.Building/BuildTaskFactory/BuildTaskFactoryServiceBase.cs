using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory
{
    public class BuildTaskFactoryServiceBase
        : CompactNodeService<BuildTaskFactoryServiceProvider, BuildTaskFactoryServiceBase, BuildTaskFactoryContext, BuildTaskFactoryServiceSettings>
    {
        public override BuildTaskFactoryContext GetEmptyContext(LocalServiceParam localParam, BuildTaskFactoryServiceSettings serviceSettings)
        {
            return new BuildTaskFactoryContext(localParam, serviceSettings);
        }

        public virtual WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, 
            BuildTaskFactoryContext context)
        {
            return GetServiceProvider().ProceedChildren(nodeData, context);
        }
    }
}
