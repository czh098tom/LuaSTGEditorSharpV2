using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Execution
{
    public class ExecutionException : Exception
    {
        public ExecutionException()
        {
        }

        public ExecutionException(string? message) : base(message)
        {
        }

        public ExecutionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
