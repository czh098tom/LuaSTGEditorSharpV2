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
    [NodeCreationMenu("PatternOperator", TitleKey = "linqstg_window_node_mapPattern")]
    public class MapPatternNode : LinqSTGNodeViewModel
    {
        private static readonly Contextual<Parameter> DefaultMapper = dict => new(dict);

        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputMapper { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public MapPatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);
            InputMapper = LinqSTGNodeInputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("pattern", InputPattern);
            AddInput("mapper", InputMapper);
            AddOutput("pattern", OutputPattern);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_mapPattern;

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputMapper.ValueChanged, 
                    (pattern, mapper) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.Select(d => (mapper ?? DefaultMapper).Invoke(d ?? Parameter.Empty)) 
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
