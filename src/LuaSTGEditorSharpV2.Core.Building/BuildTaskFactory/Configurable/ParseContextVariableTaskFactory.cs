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
    public class ParseContextVariableTaskFactory : ConfigurableBuildTaskFactory
    {
        [JsonProperty] public NodePropertyCapture? SourceCapture { get; private set; }
        [JsonProperty] public string SourceVariableName { get; private set; } = string.Empty;

        [JsonProperty] public string TargetVariableVariableName { get; private set; } = string.Empty;

        [JsonProperty] public string MapperVariableName { get; private set; } = string.Empty;

        public override WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            IInputSourceVariable? sourceVariable = null;
            IOutputTargetVariable? targetVariable = null;
            Func<string, string>? mapper = null;
            float weight = GetWeight(token);
            using (var variablesLevel = context.AcquireContextLevelHandle(nodeData))
            {
                var properties = GetServiceProvider()
                    .GetServicesPairForLogicalChildrenOfType<BuildTaskPropertySubService>(nodeData);
                sourceVariable = GetPropertyInChildren<IInputSourceVariable>(
                    nodeData, context, SourceVariableName, properties);
                targetVariable = GetPropertyInChildren<IOutputTargetVariable>(
                    nodeData, context, TargetVariableVariableName, properties);
                mapper = GetPropertyInChildren<Func<string, string>>(
                    nodeData, context, MapperVariableName, properties);
            }
            if (targetVariable == null) return null;
            sourceVariable ??= new ConstantSource(SourceCapture?.Capture(token) ?? string.Empty);
            return new WeightedBuildingTask(new ParseContextVariableTask(sourceVariable, targetVariable, mapper),
                weight).WithNameIfValid(NameCapture?.Capture(token));
        }
    }
}
