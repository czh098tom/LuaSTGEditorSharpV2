using LinqSTG;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    public class Sample01Node : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Repeater>?> InputRepeater { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public Sample01Node()
        {
            InputRepeater = LinqSTGNodeInputViewModel.Repeater("Repeater");
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");

            AddInput("repeater", InputRepeater);
            AddOutput("value", OutputValue);

            Name = "Sample01";
            TitleColor = NodeColors.Operator;

            OutputValue.Value = InputRepeater.ValueChanged
                .Select(repeater => Contextual.Create(dict 
                    => (repeater?.Invoke(dict) ?? RepeaterKey.Default.GetRepeater(dict))
                        .Sample01(IntervalType.HeadClosed)));
        }
    }
}
