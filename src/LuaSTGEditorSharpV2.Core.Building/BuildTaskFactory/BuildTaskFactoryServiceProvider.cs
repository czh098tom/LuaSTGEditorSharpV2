using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory
{
    [ServiceShortName("build"), ServiceName("BuildTaskFactory")]
    public class BuildTaskFactoryServiceProvider
        : CompactNodeServiceProvider<BuildTaskFactoryServiceProvider, BuildTaskFactoryServiceBase, BuildTaskFactoryContext, BuildTaskFactoryServiceSettings>
    {
        private BuildTaskFactoryServiceBase _default = new();
        protected override BuildTaskFactoryServiceBase DefaultService => _default;

        public WeightedBuildingTask? GetWeightedBuildingTaskForNode(NodeData nodeData,
            LocalServiceParam param) => GetWeightedBuildingTaskForNode(nodeData, param, ServiceSettings);

        public WeightedBuildingTask? GetWeightedBuildingTaskForNode(NodeData nodeData, 
            LocalServiceParam param, BuildTaskFactoryServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(nodeData, param, serviceSettings);
            var service = GetServiceOfNode(nodeData);
            return service.CreateBuildingTask(nodeData, ctx);
        }

        public WeightedBuildingTask? ProceedChildren(NodeData node, 
            BuildTaskFactoryContext context)
        {
            using var _ = context.AcquireContextLevelHandle(node);
            var tasks = new List<WeightedBuildingTask>();
            foreach (NodeData child in node.GetLogicalChildren())
            {
                var wbt = GetServiceOfNode(child).CreateBuildingTask(child, context);
                if (wbt != null) tasks.Add(wbt.Value);
            }
            if (tasks.Count == 0) return null;
            if (tasks.Count == 1) return tasks[0];
            return new WeightedBuildingTask(new CompositeBuildingTask(tasks.ToArray()));
        }
    }
}
