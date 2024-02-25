using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public readonly struct NodePropertyAccessToken
    {
        public readonly NodeData NodeData { get; }
        private readonly DefaultValueService _service;
        private readonly NodeContext _context;

        public NodePropertyAccessToken(NodeData nodeData, NodeContext context)
        {
            NodeData = nodeData;
            _context = context;
            _service = HostedApplicationHelper
                .GetService<DefaultValueServiceProvider>()
                .GetServiceOfNode(nodeData);
        }

        internal NodePropertyAccessToken(NodeData nodeData, NodeContext context, DefaultValueService service)
        {
            NodeData = nodeData;
            _context = context;
            _service = service;
        }

        public readonly string GetValueWithDefault(string key, string defaultValue = "")
        {
            return _service.GetValueWithDefault(NodeData, _context, key, defaultValue);
        }
    }
}
