using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public readonly struct NodePropertyAccessToken
    {
        public readonly NodeData NodeData { get; }
        public readonly NodeContext Context { get; }
        private readonly DefaultValueService _service;

        public NodePropertyAccessToken(NodeData nodeData, NodeContext context)
        {
            NodeData = nodeData;
            Context = context;
            _service = HostedApplicationHelper
                .GetService<DefaultValueServiceProvider>()
                .GetServiceOfNode(nodeData);
        }

        internal NodePropertyAccessToken(NodeData nodeData, NodeContext context, DefaultValueService service)
        {
            NodeData = nodeData;
            Context = context;
            _service = service;
        }

        public readonly string GetValueWithDefault(string key, string defaultValue = "")
        {
            return _service.GetValueWithDefault(NodeData, Context, key, defaultValue);
        }
    }
}
