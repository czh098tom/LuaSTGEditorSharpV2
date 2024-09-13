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
    public class CopyTaskFactory(BuildTaskFactoryServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ConfigurableBuildTaskFactory(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture? SourceCapture { get; private set; }
        [JsonProperty] public string SourceVariableName { get; private set; } = string.Empty;

        [JsonProperty] public NodePropertyCapture? TargetNameCapture { get; private set; }
        [JsonProperty] public string TargetNameVariableName { get; private set; } = string.Empty;

        [JsonProperty] public NodePropertyCapture? ArchivePathCapture { get; private set; }
        [JsonProperty] public string ArchivePathVariableName { get; private set; } = string.Empty;

        public override WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, nodeData, context);
            IInputSourceVariable? sourceVariable = null;
            IInputSourceVariable? targetNameVariable = null;
            IInputSourceVariable? archivePathVariable = null;
            float weight = GetWeight(token);
            using (var variablesLevel = context.AcquireContextLevelHandle(nodeData))
            {
                var properties = GetNodeServiceProvider()
                    .GetServicesPairForLogicalChildrenOfType<BuildTaskPropertySubService>(nodeData);
                sourceVariable = GetPropertyInChildren<IInputSourceVariable>(
                    nodeData, context, SourceVariableName, properties);
                targetNameVariable = GetPropertyInChildren<IInputSourceVariable>(
                    nodeData, context, TargetNameVariableName, properties);
                archivePathVariable = GetPropertyInChildren<IInputSourceVariable>(
                    nodeData, context, ArchivePathVariableName, properties);
            }
            sourceVariable ??= new ConstantSource(SourceCapture?.Capture(token) ?? string.Empty);
            targetNameVariable ??= new ConstantSource(TargetNameCapture?.Capture(token) ?? string.Empty);
            archivePathVariable ??= new ConstantSource(ArchivePathCapture?.Capture(token) ?? string.Empty);
            return new WeightedBuildingTask(new CopyTask(sourceVariable, targetNameVariable, archivePathVariable),
                weight).WithNameIfValid(NameCapture?.Capture(token));
        }
    }
}
