using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [JsonUseShortNaming]
    public interface IPropertyItemTerm
    {
        public PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context);
    }

}
