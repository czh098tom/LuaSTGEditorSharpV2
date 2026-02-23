using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Execution
{
    public abstract class ExecutionConfigServiceBase
    {
        public abstract ExecutionConfig? GetExecutionConfig(NodeData nodeData, ExecutionConfigContext context);
    }
}
