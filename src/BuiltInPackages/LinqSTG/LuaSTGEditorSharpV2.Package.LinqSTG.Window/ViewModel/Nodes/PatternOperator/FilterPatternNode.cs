using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    [NodeCreationMenu("PatternOperator", TitleKey = "linqstg_window_node_filterPattern")]
    public class FilterPatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputPredicate { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public FilterPatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);
            InputPredicate = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_predicate);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("pattern", InputPattern);
            AddInput("predicate", InputPredicate);
            AddOutput("pattern", OutputPattern);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_filterPattern;

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputPredicate.ValueChanged,
                    (pattern, predicate) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.Where(d => (predicate?.Invoke(d ?? Parameter.Empty) ?? 0f) != 0f)
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
