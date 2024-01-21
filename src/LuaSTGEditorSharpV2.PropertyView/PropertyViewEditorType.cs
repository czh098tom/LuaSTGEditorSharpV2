using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public record class PropertyViewEditorType(
        string Name,
        Dictionary<string, object>? Parameters = null)
    {
    }
}
