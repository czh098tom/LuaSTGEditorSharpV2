using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    /// <summary>
    /// Provide functionality of gathering resources from <see cref="NodeData"/>.
    /// </summary>
    public class ResourceGatheringServiceBase(ResourceGatheringServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
        : CompactNodeService<ResourceGatheringServiceProvider, ResourceGatheringServiceBase, ResourceGatheringContext, ResourceGatheringServiceSettings>(nodeServiceProvider, serviceProvider)
    {
        public override sealed ResourceGatheringContext GetEmptyContext(LocalServiceParam localSettings
            , ResourceGatheringServiceSettings serviceSettings)
        {
            return new ResourceGatheringContext(ServiceProvider, localSettings, serviceSettings);
        }

        public virtual IEnumerable<GroupedResource> GetResourcesToPackWithContext(NodeData node
            , ResourceGatheringContext context)
        {
            return GetNodeServiceProvider().ProceedChildren(node, context);
        }
    }
}
