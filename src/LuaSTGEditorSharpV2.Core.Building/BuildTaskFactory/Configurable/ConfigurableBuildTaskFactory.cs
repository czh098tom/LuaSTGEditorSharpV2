using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Building.BuildTasks;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable
{
    public class ConfigurableBuildTaskFactory : BuildTaskFactoryServiceBase
    {
        protected static TOutput? GetPropertyInChildren<TOutput>(NodeData nodeData,
            BuildTaskFactoryContext context,
            string variableName,
            IEnumerable<NodeServiceProvider<BuildTaskFactoryServiceBase>.NodeServicePair<BuildTaskPropertySubService>> properties)
            where TOutput : class
        {
            var sourcePropertyPair = properties.FirstOrDefault(p =>
                p.Service.CreateOutput(p.NodeData, context) == variableName);
            if (sourcePropertyPair.NodeData != null)
            {
                using var sourcePropertyLevel = context.AcquireContextLevelHandle(sourcePropertyPair.NodeData);
                var sources = GetServiceProvider()
                    .GetServicesPairForLogicalChildrenOfType<BuildTaskFactorySubService<TOutput>>(sourcePropertyPair.NodeData);
                var sourcePair = sources.FirstOrDefault();
                if (sourcePair.NodeData != null)
                {
                    return sourcePair.Service.CreateOutput(sourcePair.NodeData, context);
                }
            }
            return null;
        }

        [JsonProperty] public NodePropertyCapture? NameCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? WeightCapture { get; private set; }

        protected float GetWeight(NodePropertyAccessToken token)
        {
            var weight = 1.0f;
            if (float.TryParse(WeightCapture?.Capture(token), out var result))
            {
                weight = result;
            }

            return weight;
        }

        public override WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            float? weight = null;
            if (float.TryParse(WeightCapture?.Capture(token), out var result))
            {
                weight = result;
            }
            var taskResult = base.CreateBuildingTask(nodeData, context)
                ?.WithNameIfValid(NameCapture?.Capture(token), weight);
            return taskResult;
        }
    }
}
