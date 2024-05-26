using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public interface IPropertyViewTerm
    {
        public PropertyItemViewModelBase GetViewModel(NodeData nodeData, PropertyViewContext context);
        public CommandBase? ResolveCommandOfEditingNode(NodeData nodeData,
            PropertyViewContext context, string edited);
    }
}
