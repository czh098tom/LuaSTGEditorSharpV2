using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern
{
    using Pattern = global::LinqSTG.Pattern;

    public class RepeatPatternNode : LinqSTGNodeViewModel
    {
        public IntegerValueEditorViewModel InputTimesEditor { get; } = new() { RawValue = 1 };
        public LinqSTGNodeInputViewModel<Contextual<RepeaterKey>?> InputRepeaterKey { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputTimes { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public RepeatPatternNode()
        {
            InputTimes = LinqSTGNodeInputViewModel.Int("Times", InputTimesEditor);
            InputRepeaterKey = LinqSTGNodeInputViewModel.RepeaterKey("Repeater Key");
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("times", InputTimes);
            AddInput("repeater", InputRepeaterKey);
            AddOutput("pattern", OutputPattern);
            AddEditor("times", InputTimesEditor);

            Name = "Repeat Pattern";

            TitleColor = NodeColors.Pattern;

            OutputPattern.Value = InputTimes.ValueChanged
                .CombineLatest(InputRepeaterKey.ValueChanged,
                    (times, repeater) => Contextual.Create(dict =>
                        Pattern.Repeat<int>(times?.Invoke(dict) ?? 0)
                        .Select(r =>
                        {
                            var rKey = repeater?.Invoke(dict) ?? RepeaterKey.Default;
                            return new Parameter(dict)
                            {
                                [rKey.ID] = r.ID,
                                [rKey.Total] = r.Total,
                            };
                        })));
        }
    }
}
