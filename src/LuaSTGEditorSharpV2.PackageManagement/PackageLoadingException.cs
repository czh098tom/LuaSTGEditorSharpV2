using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.PackageManagement
{
    public class PackageLoadingException : Exception
    {
        public PackageLoadingException()
        {
        }

        public PackageLoadingException(string? message) : base(message)
        {
        }

        public PackageLoadingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PackageLoadingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
