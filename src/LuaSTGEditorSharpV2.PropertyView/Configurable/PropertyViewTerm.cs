using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public record class PropertyViewTerm(string Mapping, string Caption, Dictionary<string, string> LocalizedCaption, string Type = "")
    {
    }
}
