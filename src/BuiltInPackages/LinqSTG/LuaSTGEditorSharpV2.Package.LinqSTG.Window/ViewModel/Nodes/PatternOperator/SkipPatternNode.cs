using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    [NodeCreationMenu("Pattern/Operator", TitleKey = "linqstg_window_node_skipPattern", Order = 6)]
    public class SkipPatternNode : LinqSTGNodeViewModel
    {
        public IntegerValueEditorViewModel InputCountEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputCount { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public SkipPatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);
            InputCount = LinqSTGNodeInputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_count, InputCountEditor);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("pattern", InputPattern);
            AddInput("count", InputCount);
            AddOutput("pattern", OutputPattern);
            AddEditor("count", InputCountEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_skipPattern;

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputCount.ValueChanged,
                    (pattern, count) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.Skip(count?.Invoke(dict) ?? 0)
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
