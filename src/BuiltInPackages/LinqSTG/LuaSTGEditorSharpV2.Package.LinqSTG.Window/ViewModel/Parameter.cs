using System;
using System.Collections.Generic;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// 求值环境，作为 <see cref="Contextual{T}"/> 的通道。
    /// 由三层强类型隔间组成：
    ///   - <see cref="Floats"/>：命名浮点参数层（原 Dictionary 职责，AssignNode/Repeat 写入，TakeVariable 读取）。
    ///   - <see cref="Transform"/>：采样求值层（FromPointMovementNode 按采样时间求值点子图时设置，InputTimeNode 读取），默认 null。
    ///   - <see cref="Randomizer"/>：脚本级随机源（对应 DemoScript.TestRandom 顶部的 var randomizer），
    ///     MainViewModel 每次物化按当前种子新建并挂到根环境，随拷贝构造传播；随机节点从这里抽取。
    /// 各层不共享存储，强类型隔离，无字符串 key 冲突。
    /// 拷贝语义沿袭原 Dictionary 实现：<see cref="Parameter(Parameter)"/> 拷贝 Floats。
    /// </summary>
    public class Parameter
    {
        public static readonly Parameter Empty = new();

        /// <summary>命名浮点参数层（角色 C：参数存取 / pattern 元素载体）。</summary>
        public FloatScope Floats { get; internal set; }

        /// <summary>采样求值层（角色 D），仅在 FromPointMovementNode 按采样时间求值点子图期间非 null。</summary>
        public TransformContext? Transform { get; internal set; }

        /// <summary>
        /// 本次物化的随机源：同一实例跨子弹按枚举序推进（每颗子弹抽到各自的值），
        /// 拖动进度条只做 Predict、不再触碰它。挂同一种子的新实例即整组复现。
        /// </summary>
        public Random Randomizer { get; set; }

        public Parameter()
        {
            Floats = new FloatScope();
            Randomizer = new Random(0);
        }

        /// <summary>拷贝构造（沿袭原 new Parameter(dict) 语义）：共享同一个随机源实例。</summary>
        public Parameter(Parameter outer)
        {
            Floats = new FloatScope(outer.Floats);
            Transform = outer.Transform;
            Randomizer = outer.Randomizer;
        }

        /// <summary>
        /// 派生：叠加一个 <see cref="TransformContext"/> 层，返回新对象（不改原环境）。
        /// 供 FromPointMovementNode 在每个采样时间 t 处构造带当前 t 的环境。
        /// </summary>
        public Parameter WithTransform(TransformContext context) =>
            new Parameter(this) { Transform = context };
    }
}
