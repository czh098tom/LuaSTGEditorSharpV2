using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Building;
using LuaSTGEditorSharpV2.Core.Building.BuildTasks;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.BuildTaskFactory
{
    public class DefaultLuaSTGBuildingTaskFactory(BuildTaskFactoryServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : BuildTaskFactoryServiceBase(nodeServiceProvider, serviceProvider)
    {
        protected static readonly string _defaultOutputNameIfNotConfiguredForService = "Output";
        protected static readonly string _defaultTaskNameIfNotConfiguredForService = "Default";

        protected static readonly string _outputTargetVariableKey = "OUTPUT_TARGET";
        protected static readonly string _codeGenerationVariableKey = "GENERATED_CODE";
        protected static readonly string _gatheredResourcePathVariableKey = "GATHERED_RESOURCE_PATH";
        protected static readonly string _gatheredResourceTargetNameVariableKey = "GATHERED_RESOURCE_TARGET";
        protected static readonly string _rootLuaVariableKey = "ROOT_LUA";

        protected static readonly string _entryPointName = "_editor_output.lua";
        protected static readonly string _packageSupplementarySourceName = "package\\LegacyNode\\root.lua";
        protected static readonly string _packageSupplementaryTargetName = "root.lua";

        protected static readonly string _buildTargetDirSettingsJPath = "build.target_dir";

        [JsonProperty] public NodePropertyCapture? OutputNameCapture { get; private set; }
        [JsonProperty] public NodePropertyCapture? TaskNameCapture { get; private set; }

        public override WeightedBuildingTask? CreateBuildingTask(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, nodeData, context);

            var outputName = OutputNameCapture?.Capture(token) ?? _defaultOutputNameIfNotConfiguredForService;
            var taskName = TaskNameCapture?.Capture(token) ?? _defaultTaskNameIfNotConfiguredForService;

            var task = new CompositeBuildingTask(
                new ParseContextVariableTask(new EditorPathSource(),
                    new TargetToContext(_rootLuaVariableKey), s => Path.Combine(s, _packageSupplementarySourceName)),
                new ParseContextVariableTask(new SourceFromSettings(_buildTargetDirSettingsJPath),
                    new TargetToContext(_outputTargetVariableKey), s => Path.Combine(s, outputName)),
                new CodeGenerationTask(new DocumentPathSource(),
                    new TargetToContext(_codeGenerationVariableKey)),
                new CaptureResourceGroupTask(new DocumentPathSource(),
                    new TargetToContext(_gatheredResourcePathVariableKey),
                    new TargetToContext(_gatheredResourceTargetNameVariableKey)),
                new CopyTask(new SourceFromContext(_codeGenerationVariableKey),
                    new ConstantSource(_entryPointName),
                    new SourceFromContext(_outputTargetVariableKey)),
                new CopyTask(new SourceFromContext(_gatheredResourcePathVariableKey),
                    new SourceFromContext(_gatheredResourceTargetNameVariableKey),
                    new SourceFromContext(_outputTargetVariableKey)),
                new CopyTask(new SourceFromContext(_rootLuaVariableKey),
                    new ConstantSource(_packageSupplementaryTargetName),
                    new SourceFromContext(_outputTargetVariableKey)));
            var childTask = base.CreateBuildingTask(nodeData, context);
            return CompositeBuildingTask.LeastObjectCombine(new WeightedBuildingTask(task, 1), childTask)
                ?.WithNameIfValid(taskName);
        }
    }
}
