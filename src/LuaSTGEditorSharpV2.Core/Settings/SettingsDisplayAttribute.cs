using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsDisplayAttribute(string? name = null, float sortingOrder = 0f) : Attribute
    {
        public string? Name { get; private set; } = name;
        public float SortingOrder { get; private set; } = sortingOrder;
    }
}
