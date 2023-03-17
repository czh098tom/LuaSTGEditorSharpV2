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

        public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> values, TKey key, TValue value)
            where TKey : notnull
        {
            if (values.ContainsKey(key))
            {
                values[key] = value;
            }
            else
            {
                values.Add(key, value);
            }
        }
    }
}
