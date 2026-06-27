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
    public class MapPatternNode : LinqSTGNodeViewModel
    {
        private static readonly Contextual<Parameter> DefaultMapper = dict => new(dict);

        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputMapper { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public MapPatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern("Pattern");
            InputMapper = LinqSTGNodeInputViewModel.Transformation("Transformation");
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("pattern", InputPattern);
            AddInput("mapper", InputMapper);
            AddOutput("pattern", OutputPattern);

            Name = "Map Pattern";

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputMapper.ValueChanged, 
                    (pattern, mapper) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.Select(d => (mapper ?? DefaultMapper).Invoke(d ?? Parameter.Empty)) 
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
