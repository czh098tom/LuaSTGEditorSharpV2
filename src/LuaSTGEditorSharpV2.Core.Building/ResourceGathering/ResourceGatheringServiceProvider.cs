using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    [ServiceShortName("resg"), ServiceName("ResourceGathering")]
    public class ResourceGatheringServiceProvider
        : CompactNodeServiceProvider<ResourceGatheringServiceProvider, ResourceGatheringServiceBase, ResourceGatheringContext, ResourceGatheringServiceSettings>
    {
        private static readonly ResourceGatheringServiceBase _defaultService = new();

        protected override ResourceGatheringServiceBase DefaultService => _defaultService;

        public IEnumerable<GroupedResource> GetResourcesToPack(NodeData nodeData, LocalServiceParam localParam)
            => GetResourcesToPack(nodeData, localParam, ServiceSettings);

        public IEnumerable<GroupedResource> GetResourcesToPack(NodeData nodeData, LocalServiceParam localParam
            , ResourceGatheringServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(nodeData, localParam, serviceSettings);
            var service = GetServiceOfNode(nodeData);
            return service.GetResourcesToPackWithContext(nodeData, ctx);
        }

        public IEnumerable<GroupedResource> ProceedChildren(NodeData node, 
            ResourceGatheringContext context)
        {
            context.Push(node);
            foreach (NodeData child in node.GetLogicalChildren())
            {
                foreach (GroupedResource s in GetServiceOfNode(child).GetResourcesToPackWithContext(child, context))
                {
                    yield return s;
                }
            }
            context.Pop();
        }
    }
}
