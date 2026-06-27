using LinqSTG;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    public class TakeRepeaterFromContextNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<RepeaterKey>?> InputRepeaterKey { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Repeater>> OutputRepeater { get; }

        public TakeRepeaterFromContextNode()
        {
            InputRepeaterKey = LinqSTGNodeInputViewModel.RepeaterKey("Repeater Key");
            OutputRepeater = LinqSTGNodeOutputViewModel.Repeater("Repeater");

            AddInput("repeater_key", InputRepeaterKey);
            AddOutput("repeater", OutputRepeater);

            Name = "Take Repeater From Context";
            TitleColor = NodeColors.Operator;

            OutputRepeater.Value = InputRepeaterKey.ValueChanged
                .Select(key => Contextual.Create(dict =>
                {
                    var repeaterKey = key?.Invoke(dict) ?? RepeaterKey.Default;
                    return repeaterKey.GetRepeater(dict);
                }));
        }
    }
}
