using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    [NodeCreationMenu("Pattern/Operator", TitleKey = "linqstg_window_node_concatPattern", Order = 2)]
    public class ConcatPatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern1 { get; }
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern2 { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public ConcatPatternNode()
        {
            InputPattern1 = LinqSTGNodeInputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern1);
            InputPattern2 = LinqSTGNodeInputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern2);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("pattern1", InputPattern1);
            AddInput("pattern2", InputPattern2);
            AddOutput("pattern", OutputPattern);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_concatPattern;

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern1.ValueChanged
                .CombineLatest(InputPattern2.ValueChanged,
                    (pattern1, pattern2) => Contextual.Create(dict =>
                    {
                        var p1 = pattern1?.Invoke(dict) ?? global::LinqSTG.Pattern.Empty<Parameter, int>();
                        var p2 = pattern2?.Invoke(dict) ?? global::LinqSTG.Pattern.Empty<Parameter, int>();
                        return p1.Concat(p2);
                    }));
        }
    }
}
