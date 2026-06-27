using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    public class TakeWhilePatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputPredicate { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public TakeWhilePatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern("Pattern");
            InputPredicate = LinqSTGNodeInputViewModel.Float("Predicate");
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("pattern", InputPattern);
            AddInput("predicate", InputPredicate);
            AddOutput("pattern", OutputPattern);

            Name = "Take While Pattern";

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputPredicate.ValueChanged,
                    (pattern, predicate) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.TakeWhile(d => (predicate?.Invoke(d ?? Parameter.Empty) ?? 0f) != 0f)
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
