using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building
{
    [ServiceShortName("resg"), ServiceName("ResourceGathering")]
    public class ResourceGatheringServiceBase : NodeService<ResourceGatheringServiceBase, ResourceGatheringContext>
    {
        private static readonly ResourceGatheringServiceBase _default = new();

        static ResourceGatheringServiceBase()
        {
            _defaultServiceGetter = () => _default;
        }

        public override ResourceGatheringContext GetEmptyContext(LocalSettings localSettings)
        {
            return base.GetEmptyContext(localSettings);
        }

        protected virtual string[] GetResourcesToPackForNode(NodeData node, ResourceGatheringContext context)
        {
            return Array.Empty<string>();
        }
    }
}
