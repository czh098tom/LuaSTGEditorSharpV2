using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    [PackedServiceProvider]
    [ServiceShortName("resg"), ServiceName("ResourceGathering")]
    public class ResourceGatheringServiceProvider
        : ContextualNodeServiceProvider<ResourceGatheringServiceBase, ResourceGatheringContext, ResourceGatheringServiceSettings>
    {
        private readonly ResourceGatheringServiceBase _defaultService;

        public ResourceGatheringServiceProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _defaultService = new(this, serviceProvider);
        }

        protected override ResourceGatheringServiceBase DefaultService => _defaultService;

        public override sealed ResourceGatheringContext GetEmptyContext(LocalServiceParam localSettings
            , ResourceGatheringServiceSettings serviceSettings)
        {
            return new ResourceGatheringContext(ServiceProvider, localSettings, serviceSettings);
        }

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
            using var _ = context.AcquireContextLevelHandle(node);
            foreach (NodeData child in node.GetLogicalChildren())
            {
                foreach (GroupedResource s in GetServiceOfNode(child).GetResourcesToPackWithContext(child, context))
                {
                    yield return s;
                }
            }
        }
    }
}
