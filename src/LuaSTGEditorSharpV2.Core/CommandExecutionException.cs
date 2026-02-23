using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class CommandExecutionException(System.Exception inner) : System.Exception("", inner)
    {
        public CommandExecutionException() : this(new InvalidOperationException())
        {
        }
    }
}
