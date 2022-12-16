using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Exception
{
    public class SaveFileException : System.Exception
    {
        public SaveFileException()
        {
        }

        public SaveFileException(string? message) : base(message)
        {
        }

        public SaveFileException(string? message, System.Exception? innerException) : base(message, innerException)
        {
        }

        protected SaveFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
