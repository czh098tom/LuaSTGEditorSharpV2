using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    [NodeCreationMenu("PatternOperator", TitleKey = "linqstg_window_node_extrudePattern")]
    public class ExtrudePatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputSubPattern { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public ExtrudePatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);
            InputSubPattern = LinqSTGNodeInputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_subPattern);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("pattern", InputPattern);
            AddInput("sub_pattern", InputSubPattern);
            AddOutput("pattern", OutputPattern);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_extrudePattern;

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputSubPattern.ValueChanged,
                    (pattern, subPattern) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.SelectMany(d => (subPattern?.Invoke(d ?? Parameter.Empty) 
                            ?? global::LinqSTG.Pattern.Single<Parameter, int>(d ?? Parameter.Empty))!)
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
