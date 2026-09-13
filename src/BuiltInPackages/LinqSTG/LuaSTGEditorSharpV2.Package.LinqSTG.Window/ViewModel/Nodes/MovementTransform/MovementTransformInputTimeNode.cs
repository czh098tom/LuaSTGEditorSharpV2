using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementTransformOperator
{
    /// <summary>
    /// 采样作用域入口：输出当前采样时间 t。
    /// 仅在 <see cref="Nodes.Movement.FromPointMovementNode"/> 的点子图内有意义：
    /// 它按采样时间注入 <see cref="TransformContext"/>，本节点读 dict.Transform.Time。
    /// 作用域外返回 0（安全降级）。
    /// 配合 Math 节点可拼出时间重映射表达式 φ(t)（如 a·t、t+Δ）。
    /// </summary>
    [NodeCreationMenu("Movement/Operator", TitleKey = "linqstg_window_node_transformInputTime", Order = 5)]
    public class MovementTransformInputTimeNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputTime { get; }

        public MovementTransformInputTimeNode()
        {
            OutputTime = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_time);

            AddOutput("time", OutputTime);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_transformInputTime;
            TitleColor = NodeColors.Movement;

            OutputTime.Value = Observable.Return(
                Contextual.Create(dict => dict.Transform?.Time ?? 0f));
        }
    }
}
