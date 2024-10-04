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
        public override EditResult ResolveCommandOfEditingNode(NodeData nodeData, LocalServiceParam context, int itemIndex, string edited)
        {
            int idxCount = term.ImmutableProperty.Length;
            if (itemIndex > idxCount)
            {
                if (term.Count != null && term.VariableProperty != null)
                {
                    return new EditResult(Properties[itemIndex].ResolveEditingNodeCommand(nodeData, context, edited));
                }
                return EditResult.Empty;
            }
            else if (itemIndex <= idxCount && itemIndex >= 0)
            {
                if (term.Count != null && term.VariableProperty != null)
                {
                    return new EditResult(Properties[itemIndex].ResolveEditingNodeCommand(nodeData, context, edited), true);
                }
                return EditResult.Empty;
            }
            else if (itemIndex < idxCount && itemIndex >= 0)
            {
                return new EditResult(Properties[itemIndex].ResolveEditingNodeCommand(nodeData, context, edited));
            }
            return EditResult.Empty;
        }
    }
}
