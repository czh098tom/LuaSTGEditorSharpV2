using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class BuildActionServiceProvider
        : NodeServiceProvider<BuildActionServiceProvider, BuildActionServiceBase, BuildActionContext, BuildActionServiceSettings>
    {
        private static readonly BuildActionServiceBase _defaultService = new();

        protected override BuildActionServiceBase DefaultService => _defaultService;

        public Task BuildAsync(NodeData nodeData, LocalServiceParam settings)
            => BuildAsync(nodeData, settings, ServiceSettings);

        public async Task BuildAsync(NodeData nodeData, LocalServiceParam settings
            , BuildActionServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(nodeData, settings, serviceSettings);
            var service = GetServiceOfNode(nodeData);
            await service.BuildWithContextAsync(nodeData, ctx);
        }

        public void Build(NodeData nodeData, LocalServiceParam settings)
            => BuildAsync(nodeData, settings).RunSynchronously();

        public void Build(NodeData nodeData, LocalServiceParam settings
            , BuildActionServiceSettings serviceSettings)
            => BuildAsync(nodeData, settings, serviceSettings).RunSynchronously();

        public async Task ProceedChildrenAsync(NodeData node
            , BuildActionContext context)
        {
            context.Push(node);
            foreach (NodeData child in node.GetLogicalChildren())
            {
                await GetServiceOfNode(child).BuildWithContextAsync(child, context);
            }
            context.Pop();
        }

        public void ProceedChildren(NodeData node, BuildActionContext context)
            => ProceedChildrenAsync(node, context).RunSynchronously();
    }
}
