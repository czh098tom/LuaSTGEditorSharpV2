using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    public class RepeaterKeyNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputIDKey { get; }
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputTotalKey { get; }
        public LinqSTGNodeOutputViewModel<Contextual<RepeaterKey>> OutputRepeaterKey { get; }

        public RepeaterKeyNode()
        {
            InputIDKey = LinqSTGNodeInputViewModel.String("ID Key");
            InputTotalKey = LinqSTGNodeInputViewModel.String("Total Key");
            OutputRepeaterKey = LinqSTGNodeOutputViewModel.RepeaterKey("Repeater Key");

            AddInput("id_key", InputIDKey);
            AddInput("total_key", InputTotalKey);
            AddOutput("repeater_key", OutputRepeaterKey);

            Name = "Repeater Key";

            TitleColor = NodeColors.Data;

            OutputRepeaterKey.Value = InputIDKey.ValueChanged
                .CombineLatest(InputTotalKey.ValueChanged, (id, total) 
                    => Contextual.Create(dict => new RepeaterKey(id?.Invoke(dict) ?? "ID", 
                        total?.Invoke(dict) ?? "Total")));
        }
    }
}
