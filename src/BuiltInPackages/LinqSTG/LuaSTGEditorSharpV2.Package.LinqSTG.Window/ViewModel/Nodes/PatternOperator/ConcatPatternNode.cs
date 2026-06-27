using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    public class ConcatPatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern1 { get; }
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern2 { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public ConcatPatternNode()
        {
            InputPattern1 = LinqSTGNodeInputViewModel.Pattern("Pattern 1");
            InputPattern2 = LinqSTGNodeInputViewModel.Pattern("Pattern 2");
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("pattern1", InputPattern1);
            AddInput("pattern2", InputPattern2);
            AddOutput("pattern", OutputPattern);

            Name = "Concat Pattern";

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
