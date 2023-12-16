using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building
{
    /// <summary>
    /// Provide functionality of gathering resources from <see cref="NodeData"/>.
    /// </summary>
    public class ResourceGatheringServiceBase 
        : NodeService<ResourceGatheringServiceProvider, ResourceGatheringServiceBase, ResourceGatheringContext, ResourceGatheringServiceSettings>
    {
        public override sealed ResourceGatheringContext GetEmptyContext(LocalServiceParam localSettings
            , ResourceGatheringServiceSettings serviceSettings)
        {
            return new ResourceGatheringContext(localSettings, serviceSettings);
        }

        public virtual IEnumerable<GroupedResource> GetResourcesToPackWithContext(NodeData node
            , ResourceGatheringContext context)
        {
            return GetServiceProvider().ProceedChildren(node, context);
        }
    }
}
