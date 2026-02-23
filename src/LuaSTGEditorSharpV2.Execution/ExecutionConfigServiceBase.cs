using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Execution
{
    public abstract class ExecutionConfigServiceBase(IServiceProvider serviceProvider) : NodeServiceBase(serviceProvider)
    {
        public abstract ExecutionConfig? GetExecutionConfig(NodeData nodeData, ExecutionConfigContext context);
    }
}
