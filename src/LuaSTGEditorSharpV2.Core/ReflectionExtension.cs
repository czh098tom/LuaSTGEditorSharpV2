using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public static class ReflectionExtension
    {
        public static bool IsAnyDerivedTypeOf(this Type type, Type other)
        {
            Type? curr = type;
            while (curr != null)
            {
                if (curr == other) return true;
                curr = curr.BaseType;
            }
            return false;
        }
    }
}
