namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// 采样求值上下文：点子图当前被采样的时间 t。
    /// 仅 <see cref="Nodes.Movement.FromPointMovementNode"/> 求值点子图期间非 null：
    /// 它把新运动在每个 t 处的采样下放给子图，
    /// 使子图里的 <see cref="Nodes.MovementTransformOperator.MovementTransformInputTimeNode"/>
    /// 能读到当前 t，即统一的 Movement-Predict-FromPointMovement 自定义变换路径。
    /// 经 <see cref="Parameter"/> 通道传递，不进入 <see cref="FloatScope"/>，
    /// 避免无类型字符串 key 污染。
    /// </summary>
    public sealed class TransformContext
    {
        /// <summary>点子图当前被采样的时间 t（新运动 Predict 的入参）。
        /// 供子图表达时间重映射 φ(t)（如 a·t、t+Δ）。</summary>
        public int Time { get; init; }
    }
}
