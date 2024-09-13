using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    public class ConfigurableResourceGroupService(ResourceGatheringServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ResourceGatheringServiceBase(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public string GroupCapture { get; private set; } = string.Empty;

        public override IEnumerable<GroupedResource> GetResourcesToPackWithContext(NodeData node
            , ResourceGatheringContext context)
        {
            context.PushResourceGroup(node.GetProperty(GroupCapture));
            foreach(var gs in GetNodeServiceProvider().ProceedChildren(node, context))
            {
                yield return gs;
            }
            context.PopResourceGroup();
        }
    }
}
