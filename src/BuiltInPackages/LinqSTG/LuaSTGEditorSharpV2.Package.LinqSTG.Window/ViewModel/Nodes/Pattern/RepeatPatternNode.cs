using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern
{
    using Pattern = global::LinqSTG.Pattern;

    [NodeCreationMenu("Pattern", TitleKey = "linqstg_window_node_repeatPattern")]
    public class RepeatPatternNode : LinqSTGNodeViewModel
    {
        private static readonly Contextual<Parameter> DefaultMapper = dict => new(dict);

        public IntegerValueEditorViewModel InputTimesEditor { get; } = new() { RawValue = 1 };
        public LinqSTGNodeInputViewModel<Contextual<RepeaterKey>?> InputRepeaterKey { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputTimes { get; }
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputMapper { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public RepeatPatternNode()
        {
            InputTimes = LinqSTGNodeInputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_times, InputTimesEditor);
            InputRepeaterKey = LinqSTGNodeInputViewModel.RepeaterKey(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_repeaterKey);
            InputMapper = LinqSTGNodeInputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("times", InputTimes);
            AddInput("repeater", InputRepeaterKey);
            AddInput("mapper", InputMapper);
            AddOutput("pattern", OutputPattern);
            AddEditor("times", InputTimesEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_repeatPattern;

            TitleColor = NodeColors.Pattern;

            OutputPattern.Value = InputTimes.ValueChanged
                .CombineLatest(InputRepeaterKey.ValueChanged, InputMapper.ValueChanged,
                    (times, repeater, mapper) => Contextual.Create(dict =>
                        Pattern.Repeat<int>(times?.Invoke(dict) ?? 0)
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
