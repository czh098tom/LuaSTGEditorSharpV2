using System.Numerics;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// 变换求值期间的专属强类型作用域。仅 MovementMapNode 求值时非 null。
    /// 不进入 <see cref="FloatScope"/>，避免无类型字符串 key 污染。
    /// </summary>
    public sealed class TransformScope
    {
        /// <summary>当前正在被变换的点（强类型，非字符串 key）。</summary>
        public Vector2 Point { get; init; }

        /// <summary>当前帧（预留）。</summary>
        public int Time { get; init; }
    }
}
