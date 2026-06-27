using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    public class IntToFloatNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputInt { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputFloat { get; }

        public IntToFloatNode()
        {
            InputInt = LinqSTGNodeInputViewModel.Int("Int");
            OutputFloat = LinqSTGNodeOutputViewModel.Float("Float");

            AddInput("int", InputInt);
            AddOutput("float", OutputFloat);

            Name = "Int To Float";
            TitleColor = NodeColors.Operator;

            OutputFloat.Value = InputInt.ValueChanged
                .Select(intValue => Contextual.Create(dict => Convert.ToSingle(intValue?.Invoke(dict) ?? 0)));
        }
    }
}
