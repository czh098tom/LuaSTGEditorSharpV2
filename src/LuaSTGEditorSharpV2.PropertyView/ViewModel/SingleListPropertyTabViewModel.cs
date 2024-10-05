using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class SingleListPropertyTabViewModel<TTermVariable, TIntermediateModel>(SingleListTabTerm<TTermVariable, TIntermediateModel> term) 
        : PropertyTabViewModel
        where TTermVariable : class, IMultipleFieldPropertyViewItemTerm<TIntermediateModel>
        where TIntermediateModel : class
    {
    }
}
