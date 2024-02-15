using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class PackagePrimaryKeyAttribute(string keyPropertyName) : Attribute
    {
        public string KeyPropertyName { get; set; } = keyPropertyName;
    }
}
