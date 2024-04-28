using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public static class CollectionExtension
    {
        public static T? GetOrDefault<T>(this IReadOnlyList<T> values, int index)
        {
            if (index < values.Count)
            {
                return values[index];
            }
            else
            {
                return default;
            }
        }

        public static T GetOrDefault<T>(this IReadOnlyList<T> values, int index, T @default)
        {
            if (index < values.Count)
            {
                return values[index];
            }
            else
            {
                return @default;
            }
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, T value)
            where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
