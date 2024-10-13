using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    /// <summary>
    /// Identify a base class or interface can use short naming when serializing $type field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class JsonUseShortNamingAttribute : Attribute
    {
    }
}
