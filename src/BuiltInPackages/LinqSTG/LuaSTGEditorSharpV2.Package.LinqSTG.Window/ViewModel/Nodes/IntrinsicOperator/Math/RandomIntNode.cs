using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_randomInt", Order = 8)]
    public class RandomIntNode : LinqSTGNodeViewModel
    {
        public IntegerValueEditorViewModel InputStartEditor { get; } = new();
        public IntegerValueEditorViewModel InputEndEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputStart { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputEnd { get; }
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputValue { get; }

        public RandomIntNode()
        {
            InputStart = LinqSTGNodeInputViewModel.Int("Start", InputStartEditor);
            InputEnd = LinqSTGNodeInputViewModel.Int("End", InputEndEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            AddInput("start", InputStart);
            AddInput("end", InputEnd);
            AddOutput("value", OutputValue);
            AddEditor("start", InputStartEditor);
            AddEditor("end", InputEndEditor);
            Name = "RandomInt";
            TitleColor = NodeColors.Data;
            OutputValue.Value = InputStart.ValueChanged
                .CombineLatest(InputEnd.ValueChanged,
                    (s, e) => Contextual.Create(dict =>
                    {
                        var start = s?.Invoke(dict) ?? 0;
                        var end = e?.Invoke(dict) ?? 0;
                        return dict.Randomizer.Next(start, end + 1);
                    }));
        }
    }
}
