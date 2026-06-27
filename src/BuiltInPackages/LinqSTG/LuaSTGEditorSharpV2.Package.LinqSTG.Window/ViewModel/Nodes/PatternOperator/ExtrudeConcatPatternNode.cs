using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    public class ExtrudeConcatPatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputSubPattern { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public ExtrudeConcatPatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern("Pattern");
            InputSubPattern = LinqSTGNodeInputViewModel.Pattern("Sub Pattern");
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("pattern", InputPattern);
            AddInput("sub_pattern", InputSubPattern);
            AddOutput("pattern", OutputPattern);

            Name = "Extrude Concat Pattern";

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputSubPattern.ValueChanged,
                    (pattern, subPattern) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.SelectManyConcat(d => (subPattern?.Invoke(d ?? Parameter.Empty)
                            ?? global::LinqSTG.Pattern.Single<Parameter, int>(d ?? Parameter.Empty))!)
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
