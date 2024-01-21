using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Util
{
    public class TypedResourceDictionaryKeySelector : ResourceDictKeySelector<object>
    {
        public override string CreateKey(object vm)
        {
            return vm.GetType().Name;
        }

        public override bool HasKeyFromSource(object vm)
        {
            return true;
        }
    }
}
