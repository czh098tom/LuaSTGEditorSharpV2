using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyViewContext : NodeContext
    {
        public PropertyViewContext(LocalSettings localSettings) : base(localSettings)
        {
        }
    }
}
