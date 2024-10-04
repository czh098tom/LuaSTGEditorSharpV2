using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class BasicPropertyTabViewModel : PropertyTabViewModel
    {
        public override EditResult ResolveCommandOfEditingNode(NodeData nodeData, LocalServiceParam context, int itemIndex, string edited)
        {
            if (itemIndex < 0 || itemIndex >= Properties.Count) return EditResult.Empty;
            return new EditResult(Properties[itemIndex].ResolveEditingNodeCommand(nodeData, context, edited), true);
        }
    }
}
