using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    /// <summary>
    /// 运动变换收口节点：把连入的 <see cref="MovementTransform"/>（p ↦ p'）作用到上游运动上。
    /// 每帧求值时构造带 <see cref="TransformScope"/> 层的派生环境并重新求值变换子图，
    /// 使变换子图里的 InputPointNode 能读到当前点 p，数学节点透传该环境从而免费复用。
    /// </summary>
    public class MovementMapNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<MovementTransform>?> InputTransform { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public MovementMapNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement("Movement");
            InputTransform = LinqSTGNodeInputViewModel.MovementTransform("Transform");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("movement", InputMovement);
            AddInput("transform", InputTransform);
            AddOutput("movement", OutputMovement);

            Name = "Movement Map";

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputTransform.ValueChanged, (movement, transform)
                    => Contextual.Create(dict =>
                    {
                        var m = movement?.Invoke(dict);
                        var tfactory = transform;
                        if (tfactory == null)
                        {
                            return m ?? new Parametric<int, Vector2>(_ => Vector2.Zero);
                        }
                        return new Parametric<int, Vector2>(t =>
                        {
                            var p = (m ?? new Parametric<int, Vector2>(_ => Vector2.Zero)).Predict(t);
                            var scoped = dict.WithTransform(new TransformScope { Point = p, Time = t });
                            var tf = tfactory.Invoke(scoped) ?? MovementTransforms.Identity;
                            return tf(p);
                        });
                    }));
        }
    }
}
