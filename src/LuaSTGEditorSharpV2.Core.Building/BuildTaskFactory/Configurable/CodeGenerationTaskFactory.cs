using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Building.BuildTasks;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable
{
    public class CodeGenerationTaskFactory(BuildTaskFactoryServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ConfigurableBuildTaskFactory(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture? SourceCapture { get; private set; }
        [JsonProperty] public string SourceVariableName { get; private set; } = string.Empty;

        [JsonProperty] public string TargetVariableVariableName { get; private set; } = string.Empty;

        public override WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, nodeData, context);
            IInputSourceVariable? sourceVariable = null;
            IOutputTargetVariable? outputTargetVariable = null;
            float weight = GetWeight(token);
            using (var variablesLevel = context.AcquireContextLevelHandle(nodeData))
            {
                var properties = GetNodeServiceProvider()
                    .GetServicesPairForLogicalChildrenOfType<BuildTaskPropertySubService>(nodeData);
                sourceVariable = GetPropertyInChildren<IInputSourceVariable>(
                    nodeData, context, SourceVariableName, properties);
                outputTargetVariable = GetPropertyInChildren<IOutputTargetVariable>(
                    nodeData, context, TargetVariableVariableName, properties);
            }
            sourceVariable ??= new ConstantSource(SourceCapture?.Capture(token) ?? string.Empty);
            return new WeightedBuildingTask(new CodeGenerationTask(sourceVariable, outputTargetVariable),
                weight).WithNameIfValid(NameCapture?.Capture(token));
        }
    }
}
