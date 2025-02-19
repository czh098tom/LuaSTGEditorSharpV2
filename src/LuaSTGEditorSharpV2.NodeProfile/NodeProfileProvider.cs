using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.NodeProfile
{
    [PackedServiceProvider]
    [ServiceName("NodeProfileProvider"), ServiceShortName("profile")]
    public class NodeProfileProvider(IServiceProvider serviceProvider)
        : NodeServiceProvider<NodeProfileDescriptor>(serviceProvider)
    {
        private readonly NodeProfileDescriptor _default = new(serviceProvider);

        protected override NodeProfileDescriptor DefaultService => _default;
    }
}
