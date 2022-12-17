using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    /// <summary>
    /// Provide service name when loading service functions from assemnbly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public ServiceNameAttribute(string name)
        {
            Name = name;
        }
    }
}
