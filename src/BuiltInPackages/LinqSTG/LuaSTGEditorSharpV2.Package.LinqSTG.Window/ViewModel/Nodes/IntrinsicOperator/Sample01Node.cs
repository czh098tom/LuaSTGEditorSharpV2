using LinqSTG;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
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
        public IntervalTypeEditorViewModel IntervalTypeEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<Repeater>?> InputRepeater { get; }
        public LinqSTGNodeInputViewModel<Contextual<IntervalType>?> InputIntervalType { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public Sample01Node()
        {
            InputRepeater = LinqSTGNodeInputViewModel.Repeater("Repeater");
            InputIntervalType = LinqSTGNodeInputViewModel.IntervalType("Sample Method", IntervalTypeEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");

            AddInput("repeater", InputRepeater);
            AddInput("interval_type", InputIntervalType);
            AddOutput("value", OutputValue);
            AddEditor("interval_type", IntervalTypeEditor);

            Name = "Sample01";
            TitleColor = NodeColors.Operator;

            OutputValue.Value = InputRepeater.ValueChanged
                .CombineLatest(InputIntervalType.ValueChanged, (repeater, intervalType) =>
                    Contextual.Create(dict =>
                        (repeater?.Invoke(dict) ?? RepeaterKey.Default.GetRepeater(dict))
                            .Sample01(intervalType?.Invoke(dict) ?? IntervalType.HeadClosed)));
        }
    }
}
