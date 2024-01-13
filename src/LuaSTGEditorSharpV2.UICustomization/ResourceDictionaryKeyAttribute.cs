using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.UICustomization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ResourceDictionaryKeyAttribute(string key) : Attribute
    {
        public string Key { get; private set; } = key;
    }
}
