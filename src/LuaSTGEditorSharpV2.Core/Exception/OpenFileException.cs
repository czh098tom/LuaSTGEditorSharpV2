using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Exception
{
    public class OpenFileException : System.Exception
    {
        public OpenFileException()
        {
        }

        public OpenFileException(string? message) : base(message)
        {
        }

        public OpenFileException(string? message, System.Exception? innerException) : base(message, innerException)
        {
        }
    }
}
