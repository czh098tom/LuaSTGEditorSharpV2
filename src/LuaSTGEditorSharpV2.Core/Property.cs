using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public record class Property<T>(Func<T> Getter, Action<T> Setter)
    {
        public static implicit operator T(Property<T> property)
        {
            return property.Getter();
        }
    }
}
