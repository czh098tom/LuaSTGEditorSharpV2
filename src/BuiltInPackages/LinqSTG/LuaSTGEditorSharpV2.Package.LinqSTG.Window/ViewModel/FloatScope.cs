using System.Collections.Generic;
using System.Numerics;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// 命名浮点参数层，承担原 <see cref="Parameter"/> 作为 Dictionary 的职责。
    /// 由 AssignNode 写入、TakeVariableFromContextNode 读取。
    /// </summary>
    public class FloatScope : Dictionary<string, float>
    {
        public FloatScope() { }

        public FloatScope(IDictionary<string, float> dictionary) : base(dictionary) { }
    }

    /// <summary>
    /// 命名二维向量层：变量列表锁定项（self/player）的预览坐标，
    /// 由 MainViewModel 从变量列表种子写入、SelfPosition/PlayerPosition 节点读取。
    /// </summary>
    public class VectorScope : Dictionary<string, Vector2>
    {
        public VectorScope() { }

        public VectorScope(IDictionary<string, Vector2> dictionary) : base(dictionary) { }
    }
}
