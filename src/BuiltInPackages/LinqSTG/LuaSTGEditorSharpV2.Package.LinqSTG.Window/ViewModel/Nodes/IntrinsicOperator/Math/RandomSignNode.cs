using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_randomSign", Order = 9)]
    public class RandomSignNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputValue { get; }

        public RandomSignNode()
        {
            OutputValue = LinqSTGNodeOutputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            AddOutput("value", OutputValue);
            Name = "RandomSign";
            TitleColor = NodeColors.Data;
            OutputValue.Value = Observable.Return(Contextual.Create(dict =>
                dict.Randomizer.Next(0, 2) * 2 - 1));
        }
    }
}
