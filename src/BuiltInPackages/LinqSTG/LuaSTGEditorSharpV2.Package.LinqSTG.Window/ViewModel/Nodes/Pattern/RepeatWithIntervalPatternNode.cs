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

    [NodeCreationMenu("Pattern", TitleKey = "linqstg_window_node_repeatWithIntervalPattern", Order = 1)]
    public class RepeatWithIntervalPatternNode : LinqSTGNodeViewModel
    {
        private static readonly Contextual<Parameter> DefaultMapper = dict => new(dict);

        public IntegerValueEditorViewModel InputTimesEditor { get; } = new() { RawValue = 1 };
        public IntegerValueEditorViewModel InputIntervalEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<RepeaterKey>?> InputRepeaterKey { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputTimes { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputInterval { get; }
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputMapper { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public RepeatWithIntervalPatternNode()
        {
            InputTimes = LinqSTGNodeInputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_times, InputTimesEditor);
            InputInterval = LinqSTGNodeInputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_interval, InputIntervalEditor);
            InputRepeaterKey = LinqSTGNodeInputViewModel.RepeaterKey(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_repeaterKey);
            InputMapper = LinqSTGNodeInputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("times", InputTimes);
            AddInput("interval", InputInterval);
            AddInput("repeater", InputRepeaterKey);
            AddInput("mapper", InputMapper);
            AddOutput("pattern", OutputPattern);
            AddEditor("times", InputTimesEditor);
            AddEditor("interval", InputIntervalEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_repeatWithIntervalPattern;

            TitleColor = NodeColors.Pattern;

            OutputPattern.Value = InputTimes.ValueChanged
                .CombineLatest(InputInterval.ValueChanged, InputRepeaterKey.ValueChanged, InputMapper.ValueChanged,
                    (times, interval, repeater, mapper) => Contextual.Create(dict =>
                        Pattern.RepeatWithInterval(times?.Invoke(dict) ?? 0, interval?.Invoke(dict) ?? 0)
                        .Select(r =>
                        {
                            var rKey = repeater?.Invoke(dict) ?? RepeaterKey.Default;
                            return new Parameter(dict)
                            {
                                Floats =
                                {
                                    [rKey.ID] = r.ID,
                                    [rKey.Total] = r.Total,
                                }
                            };
                        })
                        .Select(d => (mapper ?? DefaultMapper).Invoke(d ?? Parameter.Empty))));
        }
    }
}
