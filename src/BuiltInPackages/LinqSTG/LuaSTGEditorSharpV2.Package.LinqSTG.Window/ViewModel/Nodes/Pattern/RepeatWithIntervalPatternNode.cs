using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern
{
    using Pattern = global::LinqSTG.Pattern;

    public class RepeatWithIntervalPatternNode : LinqSTGNodeViewModel
    {
        public IntegerValueEditorViewModel InputTimesEditor { get; } = new() { RawValue = 1 };
        public IntegerValueEditorViewModel InputIntervalEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<RepeaterKey>?> InputRepeaterKey { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputTimes { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputInterval { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public RepeatWithIntervalPatternNode()
        {
            InputTimes = LinqSTGNodeInputViewModel.Int("Times", InputTimesEditor);
            InputInterval = LinqSTGNodeInputViewModel.Int("Interval", InputIntervalEditor);
            InputRepeaterKey = LinqSTGNodeInputViewModel.RepeaterKey("Repeater Key");
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("times", InputTimes);
            AddInput("interval", InputInterval);
            AddInput("repeater", InputRepeaterKey);
            AddOutput("pattern", OutputPattern);
            AddEditor("times", InputTimesEditor);
            AddEditor("interval", InputIntervalEditor);

            Name = "Repeat with Interval Pattern";

            TitleColor = NodeColors.Pattern;

            OutputPattern.Value = InputTimes.ValueChanged
                .CombineLatest(InputInterval.ValueChanged, InputRepeaterKey.ValueChanged,
                    (times, interval, repeater) => Contextual.Create(dict =>
                        Pattern.RepeatWithInterval(times?.Invoke(dict) ?? 0, interval?.Invoke(dict) ?? 0)
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
