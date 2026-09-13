using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_randomFloat", Order = 7)]
    public class RandomFloatNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputStartEditor { get; } = new();
        public FloatValueEditorViewModel InputEndEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputStart { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputEnd { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public RandomFloatNode()
        {
            InputStart = LinqSTGNodeInputViewModel.Float("Start", InputStartEditor);
            InputEnd = LinqSTGNodeInputViewModel.Float("End", InputEndEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            AddInput("start", InputStart);
            AddInput("end", InputEnd);
            AddOutput("value", OutputValue);
            AddEditor("start", InputStartEditor);
            AddEditor("end", InputEndEditor);
            Name = "RandomFloat";
            TitleColor = NodeColors.Data;
            OutputValue.Value = InputStart.ValueChanged
                .CombineLatest(InputEnd.ValueChanged,
                    (s, e) => Contextual.Create(dict =>
                    {
                        var start = s?.Invoke(dict) ?? 0f;
                        var end = e?.Invoke(dict) ?? 0f;
                        return dict.Randomizer.NextSingle() * (end - start) + start;
                    }));
        }
    }
}
