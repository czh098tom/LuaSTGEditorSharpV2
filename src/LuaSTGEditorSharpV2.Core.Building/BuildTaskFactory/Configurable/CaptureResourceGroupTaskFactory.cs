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
    public class CaptureResourceGroupTaskFactory(BuildTaskFactoryServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ConfigurableBuildTaskFactory(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture? SourceCapture { get; private set; }
        [JsonProperty] public string SourceVariableName { get; private set; } = string.Empty;

        [JsonProperty] public string GatheredSourcePathVariableName { get; private set; } = string.Empty;

        [JsonProperty] public string GatheredNameInPackageVariableName { get; private set; } = string.Empty;

        public override WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, nodeData, context);
            IInputSourceVariable? sourceVariable = null;
            IOutputTargetVariable? outputTargetVariable = null;
            IOutputTargetVariable? nameInPackageVariable = null;
            float weight = GetWeight(token);
            using (var variablesLevel = context.AcquireContextLevelHandle(nodeData))
            {
                var properties = GetNodeServiceProvider()
                    .GetServicesPairForLogicalChildrenOfType<BuildTaskPropertySubService>(nodeData);
                sourceVariable = GetPropertyInChildren<IInputSourceVariable>(
                    nodeData, context, SourceVariableName, properties);
                outputTargetVariable = GetPropertyInChildren<IOutputTargetVariable>(
                    nodeData, context, GatheredSourcePathVariableName, properties);
                nameInPackageVariable = GetPropertyInChildren<IOutputTargetVariable>(
                    nodeData, context, GatheredNameInPackageVariableName, properties);
            }
            if (outputTargetVariable == null || nameInPackageVariable == null) return null;
            sourceVariable ??= new ConstantSource(SourceCapture?.Capture(token) ?? string.Empty);
            return new WeightedBuildingTask(new CaptureResourceGroupTask(sourceVariable, outputTargetVariable, nameInPackageVariable),
                weight).WithNameIfValid(NameCapture?.Capture(token));
        }
    }
}
