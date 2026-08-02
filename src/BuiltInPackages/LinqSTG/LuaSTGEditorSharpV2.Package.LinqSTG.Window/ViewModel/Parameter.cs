using System.Collections.Generic;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// 求值环境，作为 <see cref="Contextual{T}"/> 的通道。
    /// 由两层强类型隔间组成：
    ///   - <see cref="Floats"/>：命名浮点参数层（原 Dictionary 职责，AssignNode/Repeat 写入，TakeVariable 读取）。
    ///   - <see cref="Transform"/>：变换求值层（MovementMapNode 求值时设置，InputPointNode 读取），默认 null。
    /// 两层不共享存储，强类型隔离，无字符串 key 冲突。
    /// 拷贝语义沿袭原 Dictionary 实现：<see cref="Parameter(Parameter)"/> 拷贝 Floats。
    /// </summary>
    public class Parameter
    {
        public static readonly Parameter Empty = new();

        /// <summary>命名浮点参数层（角色 C：参数存取 / pattern 元素载体）。</summary>
        public FloatScope Floats { get; internal set; }

        /// <summary>变换求值层（角色 D），仅在变换收口节点求值期间非 null。</summary>
        public TransformScope? Transform { get; internal set; }

        public Parameter()
        {
            Floats = new FloatScope();
        }

        /// <summary>拷贝构造（沿袭原 new Parameter(dict) 语义）。</summary>
        public Parameter(Parameter outer)
        {
            Floats = new FloatScope(outer.Floats);
            Transform = outer.Transform;
        }

        /// <summary>
        /// 派生：叠加一个 <see cref="TransformScope"/> 层，返回新对象（不改原环境）。
        /// 供变换收口节点在每帧求值时构造带当前点 p 的环境。
        /// </summary>
        public Parameter WithTransform(TransformScope scope) =>
            new Parameter(this) { Transform = scope };
    }
}
