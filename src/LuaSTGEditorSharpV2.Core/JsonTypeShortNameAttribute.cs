using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class JsonTypeShortNameAttribute(Type sourceType, string name) : Attribute
    {
        public Type SourceType { get; } = sourceType;
        public string Name { get; } = name;
    }
}
