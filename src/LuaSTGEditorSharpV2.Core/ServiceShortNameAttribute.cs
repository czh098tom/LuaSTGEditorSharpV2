using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    /// <summary>
    /// Provide short name when loading service for given node.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceShortNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public ServiceShortNameAttribute(string name)
        {
            Name = name;
        }
    }
}
