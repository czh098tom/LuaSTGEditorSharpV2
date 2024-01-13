using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public static class ReflectionExtension
    {
        public static bool IsAnyDerivedTypeOf(this Type type, Type other)
        {
            if (!other.IsInterface)
            {
                Type? curr = type;
                while (curr != null)
                {
                    if (curr == other) return true;
                    curr = curr.BaseType;
                }
                return false;
            }
            else
            {
                Type? curr = type;
                while (curr != null)
                {
                    if (curr.ImplementedInterface(other)) return true;
                    curr = curr.BaseType;
                }
                return false;
            }
        }

        public static IEnumerable<Type> BaseTypes(this Type type)
        {
            Type? curr = type;
            while (curr != null)
            {
                yield return curr;
                curr = curr.BaseType;
            }
        }

        private static bool ImplementedInterface(this Type type, Type interfaceType)
        {
            if (type == null) return false;
            if (type == interfaceType) return true;
            return type.GetInterfaces().Any(type => type.ImplementedInterface(interfaceType));
        }
    }
}
