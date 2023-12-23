using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Exception
{
    public class PackageLoadingException : System.Exception
    {
        public PackageLoadingException()
        {
        }

        public PackageLoadingException(string? message) : base(message)
        {
        }

        public PackageLoadingException(string? message, System.Exception? innerException) : base(message, innerException)
        {
        }
    }
}
