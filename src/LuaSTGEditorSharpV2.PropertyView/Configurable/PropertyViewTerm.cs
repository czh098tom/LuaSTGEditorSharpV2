using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public record class PropertyViewTerm(
        string Mapping, 
        string Caption, 
        Dictionary<string, string> LocalizedCaption, 
        PropertyViewEditorType? Editor = null)
    {
    }
}
