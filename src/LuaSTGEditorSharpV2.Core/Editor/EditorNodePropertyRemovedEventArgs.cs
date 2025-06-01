using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Editor
{
    public class EditorNodePropertyRemovedEventArgs(string key) : EventArgs
    {
        public string Key { get; } = key;
    }
}
