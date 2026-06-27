using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    public class TakePatternNode : LinqSTGNodeViewModel
    {
        public IntegerValueEditorViewModel InputCountEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputCount { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public TakePatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern("Pattern");
            InputCount = LinqSTGNodeInputViewModel.Int("Count", InputCountEditor);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("pattern", InputPattern);
            AddInput("count", InputCount);
            AddOutput("pattern", OutputPattern);
            AddEditor("count", InputCountEditor);

            Name = "Take Pattern";

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .CombineLatest(InputCount.ValueChanged,
                    (pattern, count) => Contextual.Create(dict =>
                        pattern?.Invoke(dict)?.Take(count?.Invoke(dict) ?? 0)
                            ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
