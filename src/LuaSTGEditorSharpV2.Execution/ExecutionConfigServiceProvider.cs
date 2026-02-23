using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Execution
{
    [PackedServiceProvider]
    [ServiceName("Execution"), ServiceShortName("execfg")]
    public class ExecutionConfigServiceProvider(IServiceProvider serviceProvider) : ContextualNodeServiceProvider<ExecutionConfigServiceBase, ExecutionConfigContext, ExecutionConfigServiceSettings>(serviceProvider)
    {
        private readonly ExecutionConfigServiceBase _defaultService = new DefaultExecutionConfigService(serviceProvider);

        protected override ExecutionConfigServiceBase DefaultService => _defaultService;

        public override sealed ExecutionConfigContext GetEmptyContext(LocalServiceParam localSettings
            , ExecutionConfigServiceSettings serviceSettings)
        {
            return new ExecutionConfigContext(ServiceProvider, localSettings, serviceSettings);
        }

        public ExecutionConfig? GetExecutionConfigOfNode(NodeData nodeData, LocalServiceParam localParam)
            => GetServiceOfNode(nodeData).GetExecutionConfig(nodeData, GetContextOfNode(nodeData, localParam));
    }
}
