using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Editor
{
    public class EditorNodePropertyChangedEventArgs(string key, string oldValue, string newValue) : EventArgs
    {
        public string Key { get; } = key;
        public string OldValue { get; } = oldValue;
        public string NewValue { get; } = newValue;
    }
}
