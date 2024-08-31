using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    public class ConfigurableResourceGatheringService(ResourceGatheringServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ResourceGatheringServiceBase(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture? PathCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? TargetNameCapture { get; private set; }

        public override IEnumerable<GroupedResource> GetResourcesToPackWithContext(NodeData node
            , ResourceGatheringContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, node, context);
            string path = PathCapture?.Capture(token) ?? string.Empty;
            string target = TargetNameCapture?.Capture(token) ?? Path.GetFileName(path);
            if (!string.IsNullOrWhiteSpace(path))
            {
                foreach (var s in context.EnumerateResourceGroups())
                {
                    yield return new GroupedResource(path, target, s);
                }
            }
            foreach (var rs in GetNodeServiceProvider().ProceedChildren(node, context))
            {
                yield return rs;
            }
        }
    }
}
