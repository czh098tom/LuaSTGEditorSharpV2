using System.Collections.Generic;

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
}
