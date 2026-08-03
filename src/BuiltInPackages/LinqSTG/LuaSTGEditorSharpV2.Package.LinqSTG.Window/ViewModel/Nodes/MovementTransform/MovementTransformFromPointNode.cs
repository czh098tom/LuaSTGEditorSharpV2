using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementTransformOperator
{
    /// <summary>
    /// 分量链收口节点：把一个用户用 InputPoint + Math 节点拼出的 Vector2 子图，
    /// 包装成一个忽略输入 p、直接返回该 Vector2 的 <see cref="MovementTransform"/>。
    /// 典型用法：InputPoint → Vector2Split → [Cos/Sin 等 Math 加工] → Vector2Node → 本节点 → MovementMap.Transform。
    /// </summary>
    public class MovementTransformFromPointNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputPoint { get; }
        public LinqSTGNodeOutputViewModel<Contextual<MovementTransform>> OutputTransform { get; }

        public MovementTransformFromPointNode()
        {
            InputPoint = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_point);
            OutputTransform = LinqSTGNodeOutputViewModel.MovementTransform(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transform);

            AddInput("point", InputPoint);
            AddOutput("transform", OutputTransform);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_transformFromPoint;
            TitleColor = NodeColors.Transformation;

            OutputTransform.Value = InputPoint.ValueChanged
                .Select(pt => Contextual.Create(dict =>
                {
                    var q = pt?.Invoke(dict) ?? Vector2.Zero;
                    return (MovementTransform)(_ => q);
                }));
        }
    }
}
