using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    public class ConstantIntNode : LinqSTGNodeViewModel
    {
        public IntegerValueEditorViewModel ValueEditor { get; } = new IntegerValueEditorViewModel();
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputValue { get; }

        public ConstantIntNode()
        {
            OutputValue = LinqSTGNodeOutputViewModel.Int(editor: ValueEditor);

            AddOutput("value", OutputValue);
            AddEditor("value", ValueEditor);

            Name = "Int";

            TitleColor = NodeColors.Data;
        }
    }
}
