using LinqSTG;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    /// <summary>
    /// <see cref="Sample01MinMaxNode"/> 的二维向量版本：把循环变量归一化到 [0,1] 后，
    /// 在两个二维向量端点之间做分量线性插值（t·(ub-lb)+lb）。
    /// 未连接的向量端点按零向量处理，与代码生成的零向量降级保持一致
    /// （同 <see cref="Data.RotateVectorNode"/>）。
    /// </summary>
    [NodeCreationMenu("Operator", TitleKey = "linqstg_window_node_sample01MinMaxVector2", Order = 7)]
    public class Sample01MinMaxVector2Node : LinqSTGNodeViewModel
    {
        public IntervalTypeEditorViewModel IntervalTypeEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<Repeater>?> InputRepeater { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputLowerBound { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InpuUpperBound { get; }
        public LinqSTGNodeInputViewModel<Contextual<IntervalType>?> InputIntervalType { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputVector2 { get; }

        public Sample01MinMaxVector2Node()
        {
            InputRepeater = LinqSTGNodeInputViewModel.Repeater(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_repeater);
            InputLowerBound = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_lowerBound);
            InpuUpperBound = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_upperBound);
            InputIntervalType = LinqSTGNodeInputViewModel.IntervalType(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_sampleMethod, IntervalTypeEditor);
            OutputVector2 = LinqSTGNodeOutputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_vector2);

            AddInput("repeater", InputRepeater);
            AddInput("lower_bound", InputLowerBound);
            AddInput("upper_bound", InpuUpperBound);
            AddInput("interval_type", InputIntervalType);
            AddOutput("vector2", OutputVector2);
            AddEditor("interval_type", IntervalTypeEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_sample01MinMaxVector2;
            TitleColor = NodeColors.Operator;

            OutputVector2.Value = InputRepeater.ValueChanged
                .CombineLatest(InputLowerBound.ValueChanged, InpuUpperBound.ValueChanged, InputIntervalType.ValueChanged,
                    (repeater, lowerBound, upperBound, intervalType) => Contextual.Create(dict =>
                    {
                        var t = (repeater?.Invoke(dict) ?? RepeaterKey.Default.GetRepeater(dict))
                            .Sample01(intervalType?.Invoke(dict) ?? IntervalType.HeadClosed);
                        var lo = lowerBound?.Invoke(dict) ?? Vector2.Zero;
                        var hi = upperBound?.Invoke(dict) ?? Vector2.Zero;
                        return t * (hi - lo) + lo;
                    }, OutputVector2));
        }
    }
}
