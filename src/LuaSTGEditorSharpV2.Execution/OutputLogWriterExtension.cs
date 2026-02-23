using LuaSTGEditorSharpV2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Execution
{
    public static class OutputLogWriterExtension
    {
        public static IExecutionLogWriter ToExecutionLogWriter(this IOutputLogWriter outputLogWriter)
        {
            return ExecutionLogWriter.FromOutputLogWriter(outputLogWriter);
        }
    }
}
