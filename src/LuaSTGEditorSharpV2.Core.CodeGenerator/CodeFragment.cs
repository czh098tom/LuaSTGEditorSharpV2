using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public sealed class CodeFragment : IReadOnlyList<CodeData>
    {
        private readonly CodeData[] _items;

        public CodeFragment(IEnumerable<CodeData> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            _items = items.ToArray();
        }

        public int Count => _items.Length;
        public CodeData this[int index] => _items[index];

        public IEnumerator<CodeData> GetEnumerator() => ((IEnumerable<CodeData>)_items).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
