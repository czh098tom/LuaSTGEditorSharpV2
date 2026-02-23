using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Execution
{
    public class DefaultExecutionConfigService(IServiceProvider serviceProvider) : ExecutionConfigServiceBase(serviceProvider)
    {
        public override ExecutionConfig? GetExecutionConfig(NodeData nodeData, ExecutionConfigContext context)
        {
            return null;
        }
    }
}
