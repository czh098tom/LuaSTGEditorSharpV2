using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    /// <summary>
    /// Identify when deserialize to a base type, using the given name for $type field is also supported.
    /// </summary>
    /// <param name="baseType"> Base type, can be a class or an interface. Must have <see cref="JsonUseShortNamingAttribute"/>. </param>
    /// <param name="name"> The given name, must not be duplicated among all derived types. </param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class JsonTypeShortNameAttribute(Type baseType, string name) : Attribute
    {
        public Type BaseType { get; } = baseType;
        public string Name { get; } = name;
    }
}
