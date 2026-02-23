using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Execution
{
    public interface IExecutionLogWriter
    {
        public void WriteLine(string text);
    }
}
