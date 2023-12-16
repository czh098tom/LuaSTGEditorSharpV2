using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public interface IMultipleFieldPropertyViewItemTerm<TIntermediateModel>
        where TIntermediateModel : class
    {
        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(NodeData nodeData, int count);

        public CommandBase? GetCommand(NodeData nodeData, TIntermediateModel intermediateModel, int index);
    }
}
