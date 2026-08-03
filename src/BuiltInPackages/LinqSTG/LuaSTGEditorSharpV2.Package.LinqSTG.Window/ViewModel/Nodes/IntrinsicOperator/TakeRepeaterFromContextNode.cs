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
            InputRepeaterKey = LinqSTGNodeInputViewModel.RepeaterKey(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_repeaterKey);
            OutputRepeater = LinqSTGNodeOutputViewModel.Repeater(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_repeater);

            AddInput("repeater_key", InputRepeaterKey);
            AddOutput("repeater", OutputRepeater);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_takeRepeaterFromContext;
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
