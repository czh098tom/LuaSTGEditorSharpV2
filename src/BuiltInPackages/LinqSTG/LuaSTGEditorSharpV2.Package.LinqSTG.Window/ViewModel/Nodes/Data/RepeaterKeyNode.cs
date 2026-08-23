using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_repeaterKey", Order = 3)]
    public class RepeaterKeyNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputIDKey { get; }
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputTotalKey { get; }
        public LinqSTGNodeOutputViewModel<Contextual<RepeaterKey>> OutputRepeaterKey { get; }

        public RepeaterKeyNode()
        {
            InputIDKey = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_idKey);
            InputTotalKey = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_totalKey);
            OutputRepeaterKey = LinqSTGNodeOutputViewModel.RepeaterKey(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_repeaterKey);

            AddInput("id_key", InputIDKey);
            AddInput("total_key", InputTotalKey);
            AddOutput("repeater_key", OutputRepeaterKey);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_repeaterKey;

            TitleColor = NodeColors.Data;

            OutputRepeaterKey.Value = InputIDKey.ValueChanged
                .CombineLatest(InputTotalKey.ValueChanged, (id, total) 
                    => Contextual.Create(dict => new RepeaterKey(id?.Invoke(dict) ?? "ID", 
                        total?.Invoke(dict) ?? "Total")));
        }
    }
}
