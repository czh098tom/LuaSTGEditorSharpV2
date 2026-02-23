using LuaSTGEditorSharpV2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Execution
{
    public class ExecutionLogWriter(Action<string> writer) : IExecutionLogWriter
    {
        public static IExecutionLogWriter Empty { get; } = new ExecutionLogWriter(_ => { });

        public void WriteLine(string text)
        {
            writer.Invoke(text);
        }

        public static IExecutionLogWriter FromOutputLogWriter(IOutputLogWriter outputLogWriter)
        {
            return new FromOutputLog(outputLogWriter);
        }

        private class FromOutputLog(IOutputLogWriter outputLogWriter) : IExecutionLogWriter
        {
            private readonly IOutputLogWriter outputLogWriter = outputLogWriter;

            public void WriteLine(string text)
            {
                outputLogWriter.WriteLine("debug", text);
            }
        }

        public static IExecutionLogWriter Create(Action<string> writer)
        {
            return new ExecutionLogWriter(writer);
        }
    }
}
