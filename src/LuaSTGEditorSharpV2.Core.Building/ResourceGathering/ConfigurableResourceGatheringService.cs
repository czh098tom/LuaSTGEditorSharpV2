using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    public class ConfigurableResourceGatheringService : ResourceGatheringServiceBase
    {
        [JsonProperty] public string PathCapture { get; private set; } = string.Empty;

        public override IEnumerable<GroupedResource> GetResourcesToPackWithContext(NodeData node
            , ResourceGatheringContext context)
        {
            string path = node.GetProperty(PathCapture);
            if (!string.IsNullOrWhiteSpace(path))
            {
                foreach (var s in context.EnumerateResourceGroups())
                {
                    yield return new GroupedResource(path, s);
                }
            }
            foreach (var rs in ProceedChildren(node, context))
            {
                yield return rs;
            }
        }
    }
}
