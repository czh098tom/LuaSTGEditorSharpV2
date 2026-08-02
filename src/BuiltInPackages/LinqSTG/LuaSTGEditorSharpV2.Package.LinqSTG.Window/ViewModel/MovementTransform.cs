using System.Numerics;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// 空间变换函数：把一个点映射为另一个点。
    /// 作为图上一等可连接值（端口类型 <c>Contextual&lt;MovementTransform&gt;</c>），
    /// 用户可用变换原语节点链式组合，或用分量原语 + 数学节点自由拼装任意 <c>p ↦ p'</c>。
    /// 由 <c>MovementMapNode</c> 收口应用到运动上：<c>movement.Select(p => transform(p))</c>。
    /// </summary>
    public delegate Vector2 MovementTransform(Vector2 point);

    public static class MovementTransforms
    {
        public static readonly MovementTransform Identity = p => p;
    }
}
