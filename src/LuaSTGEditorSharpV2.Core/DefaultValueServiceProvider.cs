using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    [ServiceName("DefaultValue"), ServiceShortName("default")]
    public partial class DefaultValueServiceProvider : NodeServiceProvider<DefaultValueService>
    {
        private readonly DefaultValueService _default;

        protected override DefaultValueService DefaultService => _default;

        public DefaultValueServiceProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _default = new(serviceProvider);
        }

        public NodePropertyAccessToken GetToken(NodeData dataSource, NodeContext context)
        {
            return new NodePropertyAccessToken(dataSource, context, GetServiceOfNode(dataSource));
        }
    }
}
