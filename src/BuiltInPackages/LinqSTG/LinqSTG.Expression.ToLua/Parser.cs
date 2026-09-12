using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqSTG.Expression.ToLua
{
    public class Parser
    {
        // Monotonic counter producing unique suffixes for per-instance local
        // variable names. This prevents variable SHADOWING when an operator
        // (Add/Multiply/ScalarFunc*/movement combinator) is nested inside another
        // operator of the same family: without unique names, a child's
        // `local __lhs, __rhs` declared inside the parent's do-block would shadow
        // the parent's locals, so the parent's capture line would write the
        // child's locals and the parent would compute on nil.
        // The counter is an instance field so its scope is a single LinqSTG
        // blueprint: one Parser is constructed per blueprint translation, so the
        // generated ids stay compact and isolated per blueprint rather than
        // growing monotonically across the whole process. Graph resolution and
        // code emission are single-threaded, so no synchronization is needed.
        private int _uidCounter;

        private string GenId(string prefix)
        {
            int n = ++_uidCounter;
            return prefix + n;
        }

        /// <summary>
        /// 单个发射器（Shoot 节点）的剩余部分：__new_task/__wait 别名、
        /// __create_and_attach_movement 定义（函数体内联该发射器子树生成的 inner 代码）
        /// 加上 pattern 展开。别名声明在发射器自己的 task 作用域内而非共享前缀里：
        /// 发射器子代码是任意生成的 Lua，无法保证不会对 __new_task/__wait
        /// 非 local 赋值而覆盖别名，按发射器隔离后覆盖至多影响这一个发射器。
        /// 共享的 local __self = self 前置部分由 <see cref="ShootGroup"/> 统一输出。
        /// </summary>
        public LuaParser Shoot(LuaParser pattern, LuaParser movement)
        {
            return (inner) =>
            {
                var constructedInner = Concat(
                    Single("__create_and_attach_movement(function(self)"),
                    Single("local __t", 1),
                    Single("for _ = 1, _infinite do", 1),
                    Single("__t = self.timer", 2),
                    Single("local __x, __y = 0, 0", 2),
                    Shift(movement(inner), 2),
                    Single("self.x = __x", 2),
                    Single("self.y = __y", 2),
                    Single("task.Wait()", 2),
                    Single("end", 1),
                    Single("end)")
                );
                return Concat(
                    Single("local __new_task = function(fn) task.New(self, fn) end"),
                    Single("local __wait = task.Wait"),
                    Single("local __create_and_attach_movement = function(fn)"),
                    Shift(inner, 1),
                    Single("local self = last", 1),
                    Single("return task.New(self, function() fn(self) end)", 1),
                    Single("end"),
                    pattern(constructedInner)
                );
            };
        }

        /// <summary>
        /// 把全部发射器（Shoot 节点）的翻译组合成一段自包含代码：
        /// 整体套一层 do...end 隔离局部变量；随后是共享的 local __self = self 前置部分；
        /// 在它之后每一个发射器节点输出各自的剩余部分（<see cref="Shoot"/>），
        /// 并在外面套一层 task.New(self, function() ... end)，
        /// 使多个发射器作为独立协程并发运行（各自内部的 __wait 互不阻塞）。
        /// </summary>
        public LuaParser ShootGroup(IReadOnlyList<LuaParser> shooters)
        {
            return (inner) => Concat(
                Single("do"),
                // Redirect the shooter's self before any nested scope rebinds it:
                // the movement function's (self) parameter, __create_and_attach_movement's
                // `local self = last`, and ExtrudePattern's __new_task(function(self) ...)
                // all shadow it with the bullet object. SelfPosition reads __self.
                Single("local __self = self", 1),
                shooters.SelectMany(shooter => Concat(
                    Single("task.New(self, function()", 1),
                    Shift(shooter(inner), 2),
                    Single("end)", 1)
                )),
                Single("end")
            );
        }

        public LuaParser RepeatWithIntervalPattern(LuaParser times, LuaParser interval, LuaParser repeater)
        {
            return (inner) => Concat(
                Single("local __t"),
                Single("do"),
                Shift(times(inner), 1),
                Single("__t = __val", 1),
                Single("end"),
                Single("local __intv"),
                Single("do"),
                Shift(interval(inner), 1),
                Single("__intv = __val", 1),
                Single("end"),
                Single("for __i = 0, __t - 1 do"),
                Single($"local {FlatText(repeater(inner))} = __t, __i", 1),
                Shift(inner, 1),
                Single("__wait(__intv)", 1),
                Single("end")
            );
        }

        public LuaParser Repeater(LuaParser curr, LuaParser max)
        {
            return (inner) => Single($"{FlatText(max(inner))}, {FlatText(curr(inner))}");
        }

        public LuaParser DefaultRepeater()
        {
            return (inner) => Single($"__t, __i");
        }

        public LuaParser Sample01MinMax(LuaParser repeater, LuaParser lb, LuaParser ub, IntervalType intervalType)
        {
            return (inner) => Concat(
                Single("local __lb, __ub"),
                Single("do"),
                Shift(lb(inner), 1),
                Single("__lb = __val", 1),
                Single("end"),
                Single("do"),
                Shift(ub(inner), 1),
                Single("__ub = __val", 1),
                Single("end"),
                Single($"local __max, __curr = {FlatText(repeater(inner))}"),
                GetIntervalManipulater("__u", intervalType)(inner),
                Single("local __val = __u * (__ub - __lb) + __lb")
            );
        }

        /// <summary>
        /// <see cref="Sample01MinMax"/> 的二维向量版本：把循环变量归一化到 [0,1] 后，
        /// 在两个二维向量端点之间做分量线性插值，产出 __valx/__valy。
        /// 端点输入为 Vector2（__valx/__valy 二局部约定）。
        /// </summary>
        public LuaParser Sample01MinMaxVector2(LuaParser repeater, LuaParser lb, LuaParser ub, IntervalType intervalType)
        {
            return (inner) => Concat(
                Single("local __lbx, __lby"),
                Single("do"),
                Shift(lb(inner), 1),
                Single("__lbx, __lby = __valx, __valy", 1),
                Single("end"),
                Single("local __ubx, __uby"),
                Single("do"),
                Shift(ub(inner), 1),
                Single("__ubx, __uby = __valx, __valy", 1),
                Single("end"),
                Single($"local __max, __curr = {FlatText(repeater(inner))}"),
                GetIntervalManipulater("__u", intervalType)(inner),
                Single("local __valx = __u * (__ubx - __lbx) + __lbx"),
                Single("local __valy = __u * (__uby - __lby) + __lby")
            );
        }

        public LuaParser Sample01(LuaParser repeater, IntervalType intervalType)
        {
            return (inner) => Concat(
                Single($"local __max, __curr = {FlatText(repeater(inner))}"),
                GetIntervalManipulater("__val", intervalType)(inner)
            );
        }

        private LuaParser GetIntervalManipulater(string name, IntervalType intervalType)
        {
            return intervalType switch
            {
                IntervalType.Open => (inner) => Single($"local {name} = (__curr + 1) / (__max + 1)"),
                IntervalType.HeadClosed => (inner) => Single($"local {name} = __curr / __max"),
                IntervalType.TailClosed => (inner) => Single($"local {name} = (__curr + 1) / __max"),
                IntervalType.BothClosed => (inner) => Single($"local {name} = __curr / (__max - 1)"),
                _ => GetIntervalManipulater(name, IntervalType.HeadClosed)
            };
        }

        public LuaParser MinMax(LuaParser value, LuaParser lb, LuaParser ub)
        {
            return (inner) => Concat(
                Single("local __v"),
                Single("do"),
                Shift(value(inner), 1),
                Single("__v = __val", 1),
                Single("end"),
                Single("local __lb, __ub"),
                Single("do"),
                Shift(lb(inner), 1),
                Single("__lb = __val", 1),
                Single("end"),
                Single("do"),
                Shift(ub(inner), 1),
                Single("__ub = __val", 1),
                Single("end"),
                Single("local __val = __ub * __v + __lb * (1 - __v)")
            );
        }

        public LuaParser RepeatPattern(LuaParser times, LuaParser repeaterKey)
        {
            return (inner) =>
            {
                var repeaterText = FlatText(repeaterKey(inner));
                return Concat(
                    Single("local __t"),
                    Single("do"),
                    Shift(times(inner), 1),
                    Single("__t = __val", 1),
                    Single("end"),
                    Single("for __i = 0, __t - 1 do"),
                    Single($"local {repeaterText} = __t, __i", 1),
                    Shift(inner, 1),
                    Single("end")
                );
            };
        }

        public LuaParser TakeRepeaterFromContext(LuaParser repeaterKey)
        {
            return repeaterKey;
        }

        public LuaParser ConstantFloat(float value)
        {
            return _ => Single($"local __val = {value}");
        }

        public LuaParser ConstantString(string str)
        {
            return _ => Single(str);
        }

        public LuaParser IntrinsicAdd(LuaParser lhs, LuaParser rhs)
        {
            string l = GenId("__lhs_"), r = GenId("__rhs_");
            return (inner) => Concat(
                Single($"local {l}, {r}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{l} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{r} = __val", 1),
                Single("end"),
                Single($"local __val = {l} + {r}")
            );
        }

        public LuaParser IntrinsicAddVector2(LuaParser lhs, LuaParser rhs)
        {
            string lx = GenId("__lhsx_"), ly = GenId("__lhsy_");
            string rx = GenId("__rhsx_"), ry = GenId("__rhsy_");
            return (inner) => Concat(
                Single($"local {lx}, {ly}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{lx}, {ly} = __valx, __valy", 1),
                Single("end"),
                Single($"local {rx}, {ry}"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{rx}, {ry} = __valx, __valy", 1),
                Single("end"),
                Single($"local __valx = {lx} + {rx}"),
                Single($"local __valy = {ly} + {ry}")
            );
        }

        public LuaParser IntrinsicSubtract(LuaParser lhs, LuaParser rhs)
        {
            string l = GenId("__lhs_"), r = GenId("__rhs_");
            return (inner) => Concat(
                Single($"local {l}, {r}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{l} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{r} = __val", 1),
                Single("end"),
                Single($"local __val = {l} - {r}")
            );
        }

        public LuaParser IntrinsicSubtractVector2(LuaParser lhs, LuaParser rhs)
        {
            string lx = GenId("__lhsx_"), ly = GenId("__lhsy_");
            string rx = GenId("__rhsx_"), ry = GenId("__rhsy_");
            return (inner) => Concat(
                Single($"local {lx}, {ly}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{lx}, {ly} = __valx, __valy", 1),
                Single("end"),
                Single($"local {rx}, {ry}"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{rx}, {ry} = __valx, __valy", 1),
                Single("end"),
                Single($"local __valx = {lx} - {rx}"),
                Single($"local __valy = {ly} - {ry}")
            );
        }

        public LuaParser IntrinsicMultiply(LuaParser lhs, LuaParser rhs)
        {
            string l = GenId("__lhs_"), r = GenId("__rhs_");
            return (inner) => Concat(
                Single($"local {l}, {r}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{l} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{r} = __val", 1),
                Single("end"),
                Single($"local __val = {l} * {r}")
            );
        }

        public LuaParser IntrinsicMultiplyVector2(LuaParser lhs, LuaParser rhs)
        {
            string lx = GenId("__lhsx_"), ly = GenId("__lhsy_");
            string rx = GenId("__rhsx_"), ry = GenId("__rhsy_");
            return (inner) => Concat(
                Single($"local {lx}, {ly}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{lx}, {ly} = __valx, __valy", 1),
                Single("end"),
                Single($"local {rx}, {ry}"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{rx}, {ry} = __valx, __valy", 1),
                Single("end"),
                Single($"local __valx = {lx} * {rx}"),
                Single($"local __valy = {ly} * {ry}")
            );
        }

        public LuaParser IntrinsicDivide(LuaParser lhs, LuaParser rhs)
        {
            string l = GenId("__lhs_"), r = GenId("__rhs_");
            return (inner) => Concat(
                Single($"local {l}, {r}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{l} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{r} = __val", 1),
                Single("end"),
                Single($"local __val = {l} / {r}")
            );
        }

        public LuaParser IntrinsicDivideVector2(LuaParser lhs, LuaParser rhs)
        {
            string lx = GenId("__lhsx_"), ly = GenId("__lhsy_");
            string rx = GenId("__rhsx_"), ry = GenId("__rhsy_");
            return (inner) => Concat(
                Single($"local {lx}, {ly}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{lx}, {ly} = __valx, __valy", 1),
                Single("end"),
                Single($"local {rx}, {ry}"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{rx}, {ry} = __valx, __valy", 1),
                Single("end"),
                Single($"local __valx = {lx} / {rx}"),
                Single($"local __valy = {ly} / {ry}")
            );
        }

        public LuaParser IntrinsicModulo(LuaParser lhs, LuaParser rhs)
        {
            string l = GenId("__lhs_"), r = GenId("__rhs_");
            return (inner) => Concat(
                Single($"local {l}, {r}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{l} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{r} = __val", 1),
                Single("end"),
                Single($"local __val = {l} % {r}")
            );
        }

        public LuaParser IntrinsicNegate(LuaParser x)
        {
            string xv = GenId("__x_");
            return (inner) => Concat(
                Single($"local {xv}"),
                Single("do"),
                Shift(x(inner), 1),
                Single($"{xv} = __val", 1),
                Single("end"),
                Single($"local __val = -{xv}")
            );
        }

        public LuaParser IntrinsicNegateVector2(LuaParser x)
        {
            string xv = GenId("__xv_"), yv = GenId("__yv_");
            return (inner) => Concat(
                Single($"local {xv}, {yv}"),
                Single("do"),
                Shift(x(inner), 1),
                Single($"{xv}, {yv} = __valx, __valy", 1),
                Single("end"),
                Single($"local __valx = -{xv}"),
                Single($"local __valy = -{yv}")
            );
        }

        // Single-input scalar function: produces `local __val = <expr>(__x)`.
        // Uses a per-instance unique local name so a nested operand that also
        // binds `__x` (e.g. Negate, another ScalarFunc1) cannot shadow this one.
        public LuaParser ScalarFunc1(LuaParser x, Func<string, string> expr)
        {
            string xVar = GenId("__x_");
            return (inner) => Concat(
                Single($"local {xVar}"),
                Single("do"),
                Shift(x(inner), 1),
                Single($"{xVar} = __val", 1),
                Single("end"),
                Single($"local __val = {expr(xVar)}")
            );
        }

        // Two-input scalar function: produces `local __val = <expr>(__lhs, __rhs)`.
        // Uses per-instance unique local names (see ScalarFunc1 note).
        public LuaParser ScalarFunc2(LuaParser lhs, LuaParser rhs, Func<string, string, string> expr)
        {
            string lhsVar = GenId("__lhs_");
            string rhsVar = GenId("__rhs_");
            return (inner) => Concat(
                Single($"local {lhsVar}, {rhsVar}"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single($"{lhsVar} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single($"{rhsVar} = __val", 1),
                Single("end"),
                Single($"local __val = {expr(lhsVar, rhsVar)}")
            );
        }

        // Three-input scalar function: produces `local __val = <expr>(__a, __b, __c)`.
        // Uses per-instance unique local names (see ScalarFunc1 note).
        public LuaParser ScalarFunc3(LuaParser a, LuaParser b, LuaParser c, Func<string, string, string, string> expr)
        {
            string aVar = GenId("__a_");
            string bVar = GenId("__b_");
            string cVar = GenId("__c_");
            return (inner) => Concat(
                Single($"local {aVar}, {bVar}, {cVar}"),
                Single("do"),
                Shift(a(inner), 1),
                Single($"{aVar} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(b(inner), 1),
                Single($"{bVar} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(c(inner), 1),
                Single($"{cVar} = __val", 1),
                Single("end"),
                Single($"local __val = {expr(aVar, bVar, cVar)}")
            );
        }

        // --- Scalar math helpers (LuaSTG runtime globals, degree-based trig) ---

        public LuaParser Sin(LuaParser x) => ScalarFunc1(x, a => $"sin({a})");
        public LuaParser Cos(LuaParser x) => ScalarFunc1(x, a => $"cos({a})");
        public LuaParser Tan(LuaParser x) => ScalarFunc1(x, a => $"tan({a})");
        public LuaParser ASin(LuaParser x) => ScalarFunc1(x, a => $"asin({a})");
        public LuaParser ACos(LuaParser x) => ScalarFunc1(x, a => $"acos({a})");
        public LuaParser ATan(LuaParser x) => ScalarFunc1(x, a => $"atan({a})");
        public LuaParser ATan2(LuaParser y, LuaParser x) => ScalarFunc2(y, x, (a, b) => $"atan2({a}, {b})");
        public LuaParser DegToRad(LuaParser x) => ScalarFunc1(x, a => $"{a} * math.pi / 180");
        public LuaParser RadToDeg(LuaParser x) => ScalarFunc1(x, a => $"{a} * 180 / math.pi");
        public LuaParser Abs(LuaParser x) => ScalarFunc1(x, a => $"abs({a})");
        public LuaParser Sqrt(LuaParser x) => ScalarFunc1(x, a => $"sqrt({a})");
        public LuaParser Floor(LuaParser x) => ScalarFunc1(x, a => $"floor({a})");
        public LuaParser Ceil(LuaParser x) => ScalarFunc1(x, a => $"ceil({a})");
        public LuaParser Sign(LuaParser x) => ScalarFunc1(x, a => $"sign({a})");
        public LuaParser Exp(LuaParser x) => ScalarFunc1(x, a => $"exp({a})");
        public LuaParser Log(LuaParser x) => ScalarFunc1(x, a => $"log({a})");
        public LuaParser Pow(LuaParser b, LuaParser e) => ScalarFunc2(b, e, (a, c) => $"{a} ^ {c}");
        public LuaParser Min(LuaParser a, LuaParser b) => ScalarFunc2(a, b, (x, y) => $"min({x}, {y})");
        public LuaParser Max(LuaParser a, LuaParser b) => ScalarFunc2(a, b, (x, y) => $"max({x}, {y})");
        public LuaParser Clamp(LuaParser x, LuaParser lo, LuaParser hi) => ScalarFunc3(x, lo, hi, (a, b, c) => $"min(max({a}, {b}), {c})");
        public LuaParser Lerp(LuaParser a, LuaParser b, LuaParser t) => ScalarFunc3(a, b, t, (x, y, z) => $"{x} + ({y} - {x}) * {z}");

        // --- Random helpers (LuaSTG runtime `ran` random generator object) ---
        // ran:Float(start, end) samples [start, end); ran:Int(start, end)
        // samples [start, end] inclusive; ran:Sign() returns -1 or 1.
        public LuaParser RandomFloat(LuaParser start, LuaParser end) => ScalarFunc2(start, end, (a, b) => $"ran:Float({a}, {b})");
        public LuaParser RandomInt(LuaParser start, LuaParser end) => ScalarFunc2(start, end, (a, b) => $"ran:Int({a}, {b})");
        public LuaParser RandomSign() => (inner) => Single("local __val = ran:Sign()");

        public LuaParser UniformVelocityMovement(LuaParser vec)
        {
            string vx = GenId("__vx_"), vy = GenId("__vy_");
            return (inner) => Concat(
                Single($"local {vx}, {vy}"),
                Single("do"),
                Shift(vec(inner), 1),
                Single($"{vx}, {vy} = __valx, __valy", 1),
                Single("end"),
                Single($"__x = {vx} * __t"),
                Single($"__y = {vy} * __t")
            );
        }

        public LuaParser VectorFromAngleLength(LuaParser angle, LuaParser length)
        {
            return (inner) => Concat(
                Single("local __angle, __length"),
                Single("do"),
                Shift(angle(inner), 1),
                Single("__angle = __val", 1),
                Single("end"),
                Single("do"),
                Shift(length(inner), 1),
                Single("__length = __val", 1),
                Single("end"),
                Single("local __valx = cos(__angle) * __length"),
                Single("local __valy = sin(__angle) * __length")
            );
        }

        public LuaParser TakeVariableFromContext(LuaParser key)
        {
            return (inner) => Single($"local __val = {FlatText(key(inner))}");
        }

        /// <summary>
        /// Vector2Variable 的 vector2 输出：把两个外层作用域变量名直接读取为
        /// __valx/__valy 二维向量输出，即 TakeVariableFromContext 的二维版本。
        /// </summary>
        public LuaParser Vector2FromVariables(LuaParser xKey, LuaParser yKey)
        {
            return (inner) => Concat(
                Single($"local __valx = {FlatText(xKey(inner))}"),
                Single($"local __valy = {FlatText(yKey(inner))}")
            );
        }

        /// <summary>
        /// 外部坐标变量：把目标的 .x/.y 字段读取为 __valx/__valy 二维向量输出。
        /// self 走 Shoot 头部重定向的 __self 别名（原始 self 在运动函数内被子弹对象遮蔽），
        /// player 是宿主全局对象，直接读取。仅用于变量列表的锁定项。
        /// </summary>
        public LuaParser OuterPosition(string target)
        {
            return (inner) => Concat(
                Single($"local __valx = {target}.x"),
                Single($"local __valy = {target}.y")
            );
        }

        public LuaParser MovementAfterTime(LuaParser m1, LuaParser time, LuaParser m2)
        {
            return (inner) => Concat(
                Single("local __ts"),
                Single("do"),
                Shift(time(inner), 1),
                Single("__ts = __val", 1),
                Single("end"),

                Single("if __t < __ts then"),

                Shift(m1(inner), 1),

                Single("else"),

                Single("local __sx1, __sy1", 1),
                Single("do", 1),
                Single("local __t = __ts", 2),
                Shift(m1(inner), 2),
                Single("__sx1, __sy1 = __x, __y", 2),
                Single("end", 1),

                Single("do", 1),
                Single("local __t = __t - __ts", 2),
                Shift(m2(inner), 2),
                Single("__sx1 = __sx1 + __x", 2),
                Single("__sy1 = __sy1 + __y", 2),
                Single("end", 1),

                Single("__x = __sx1", 1),
                Single("__y = __sy1", 1),

                Single("end")
            );
        }

        public LuaParser Assign(LuaParser prev, LuaParser value, LuaParser key)
        {
            return (inner) => Concat(
                prev(inner),
                Single("local __v"),
                Single("do"),
                Shift(value(inner), 1),
                Single("__v = __val", 1),
                Single("end"),
                Single($"local {FlatText(key(inner))} = __v")
            );
        }

        /// <summary>
        /// Vector2Assignment：求值 Vector2 输入（__valx/__valy）后，把两个分量分别
        /// 赋给两个外层作用域变量名（local &lt;x&gt; = vx; local &lt;y&gt; = vy），
        /// 是 <see cref="Assign"/> 的二维版本。
        /// </summary>
        public LuaParser AssignVector2(LuaParser prev, LuaParser value, LuaParser xKey, LuaParser yKey)
        {
            string vx = GenId("__vx_"), vy = GenId("__vy_");
            return (inner) => Concat(
                prev(inner),
                Single($"local {vx}, {vy}"),
                Single("do"),
                Shift(value(inner), 1),
                Single($"{vx}, {vy} = __valx, __valy", 1),
                Single("end"),
                Single($"local {FlatText(xKey(inner))} = {vx}"),
                Single($"local {FlatText(yKey(inner))} = {vy}")
            );
        }

        public LuaParser MapPattern(LuaParser pattern, LuaParser transformation)
        {
            return (inner) => pattern(Concat(transformation(inner), inner));
        }

        public LuaParser ExtrudePattern(LuaParser pattern, LuaParser transformation)
        {
            return (inner) => pattern(Concat(
                Single("__new_task(function(self)"),
                Shift(transformation(inner), 1),
                Single("end)")
            ));
        }

        public LuaParser ExtrudeConcatPattern(LuaParser pattern, LuaParser subPattern)
        {
            return (inner) => pattern(Concat(
                Single("do"),
                Shift(subPattern(inner), 1),
                Single("end")
            ));
        }

        public LuaParser Vector2(LuaParser x, LuaParser y)
        {
            string vx = GenId("__vx_"), vy = GenId("__vy_");
            return (inner) => Concat(
                Single($"local {vx}, {vy}"),
                Single("do"),
                Shift(x(inner), 1),
                Single($"{vx} = __val", 1),
                Single("end"),
                Single("do"),
                Shift(y(inner), 1),
                Single($"{vy} = __val", 1),
                Single("end"),
                Single($"local __valx = {vx}"),
                Single($"local __valy = {vy}")
            );
        }

        /// <summary>
        /// RotateVector：把输入 Vector2（__valx/__valy）绕原点旋转角度制的角度，
        /// 产出旋转后的 __valx/__valy。cos/sin 为 LuaSTG 运行时的角度制三角函数，
        /// 旋转方向与 MovementRotate 的 C# 求值一致（正角度在数学坐标系下逆时针）。
        /// </summary>
        public LuaParser Vector2Rotate(LuaParser vec, LuaParser angle)
        {
            string vx = GenId("__vx_"), vy = GenId("__vy_");
            return (inner) => Concat(
                Single($"local {vx}, {vy}"),
                Single("do"),
                Shift(vec(inner), 1),
                Single($"{vx}, {vy} = __valx, __valy", 1),
                Single("end"),
                Single("local __angle"),
                Single("do"),
                Shift(angle(inner), 1),
                Single("__angle = __val", 1),
                Single("end"),
                Single($"local __valx = {vx} * cos(__angle) - {vy} * sin(__angle)"),
                Single($"local __valy = {vx} * sin(__angle) + {vy} * cos(__angle)")
            );
        }

        public LuaParser FloatToInt(LuaParser f)
        {
            return (inner) => Concat(
                Single("local __f"),
                Single("do"),
                Shift(f(inner), 1),
                Single("__f = __val", 1),
                Single("end"),
                Single("local __val = math.floor(__f + 0.5)")
            );
        }

        public LuaParser IntToFloat(LuaParser f)
        {
            return f;
        }

        /// <summary>
        /// 从点生成运动（原名 StationaryMovement）：把 Vector2 子图结果包装成常量运动。
        /// 是统一自定义变换路径 Movement-Predict-FromPointMovement 的收口半段：
        /// 变换子图内 Predict 采样出点、经点级运算后由本组合子包装回运动。
        /// </summary>
        public LuaParser FromPointMovement(LuaParser position)
        {
            return (inner) => Concat(
                Single("do"),
                Shift(position(inner), 1),
                Single("__x = __valx", 1),
                Single("__y = __valy", 1),
                Single("end")
            );
        }

        public LuaParser UniformAccelerationMovement(LuaParser initialVelocity, LuaParser acceleration)
        {
            return (inner) => Concat(
                Single("local __ivx, __ivy"),
                Single("do"),
                Shift(initialVelocity(inner), 1),
                Single("__ivx = __valx", 1),
                Single("__ivy = __valy", 1),
                Single("end"),
                Single("local __ax, __ay"),
                Single("do"),
                Shift(acceleration(inner), 1),
                Single("__ax = __valx", 1),
                Single("__ay = __valy", 1),
                Single("end"),
                Single("__x = __ivx * __t + __ax * __t * __t / 2"),
                Single("__y = __ivy * __t + __ay * __t * __t / 2")
            );
        }

        public LuaParser MovementSum(LuaParser m1, LuaParser m2)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(m1(inner), 1),
                Single("end"),
                // Capture/accumulate OUTSIDE the do-block: a child movement may itself
                // declare `local __sx, __sy` (Sum/Offset/Scale/Rotate), which would
                // shadow ours if the assignment stayed inside the block.
                Single("__sx, __sy = __x, __y"),
                Single("do"),
                Shift(m2(inner), 1),
                Single("end"),
                Single("__sx = __sx + __x"),
                Single("__sy = __sy + __y"),
                Single("__x = __sx"),
                Single("__y = __sy")
            );
        }

        public LuaParser MovementOffset(LuaParser movement, LuaParser offset)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(movement(inner), 1),
                Single("end"),
                Single("__sx, __sy = __x, __y"),
                Single("do"),
                Shift(offset(inner), 1),
                Single("__sx = __sx + __valx", 1),
                Single("__sy = __sy + __valy", 1),
                Single("end"),
                Single("__x = __sx"),
                Single("__y = __sy")
            );
        }

        public LuaParser MovementRotate(LuaParser movement, LuaParser angle)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(movement(inner), 1),
                Single("end"),
                Single("__sx, __sy = __x, __y"),
                Single("local __angle"),
                Single("do"),
                Shift(angle(inner), 1),
                Single("__angle = __val", 1),
                Single("end"),
                Single("__x = __sx * cos(__angle) - __sy * sin(__angle)"),
                Single("__y = __sx * sin(__angle) + __sy * cos(__angle)")
            );
        }

        public LuaParser MovementScale(LuaParser movement, LuaParser scale)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(movement(inner), 1),
                Single("end"),
                Single("__sx, __sy = __x, __y"),
                Single("local __kx, __ky"),
                Single("do"),
                Shift(scale(inner), 1),
                Single("__kx, __ky = __valx, __valy", 1),
                Single("end"),
                Single("__x = __sx * __kx"),
                Single("__y = __sy * __ky")
            );
        }

        // --- Polar coordinate movements (degree-based trig, LuaSTG runtime globals) ---

        /// <summary>
        /// 把上游运动预测点 p 当作 (角度θ=q.X 度, 半径r=q.Y) 的极坐标，
        /// 展开回笛卡尔坐标：p' = center + (r·cos(θ), r·sin(θ))，其中 q = p - center。
        /// </summary>
        public LuaParser MovementCartesianToPolar(LuaParser movement, LuaParser center)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(movement(inner), 1),
                Single("end"),
                Single("__sx, __sy = __x, __y"),
                Single("local __cx, __cy"),
                Single("do"),
                Shift(center(inner), 1),
                Single("__cx, __cy = __valx, __valy", 1),
                Single("end"),
                Single("local __qx = __sx - __cx"),
                Single("local __qy = __sy - __cy"),
                Single("__x = __cx + __qy * cos(__qx)"),
                Single("__y = __cy + __qy * sin(__qx)")
            );
        }

        /// <summary>
        /// MovementCartesianToPolar 的逆变换：把上游运动预测点 p 当作笛卡尔点 (x,y)，
        /// 转成极坐标 (半径r, 角度θ度)，p' = center + (√(x²+y²), atan2(y,x))，
        /// 其中 q = p - center，atan2 返回度（LuaSTG 运行时度制）。
        /// </summary>
        public LuaParser MovementPolarToCartesian(LuaParser movement, LuaParser center)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(movement(inner), 1),
                Single("end"),
                Single("__sx, __sy = __x, __y"),
                Single("local __cx, __cy"),
                Single("do"),
                Shift(center(inner), 1),
                Single("__cx, __cy = __valx, __valy", 1),
                Single("end"),
                Single("local __qx = __sx - __cx"),
                Single("local __qy = __sy - __cy"),
                Single("__x = __cx + sqrt(__qx * __qx + __qy * __qy)"),
                Single("__y = __cy + atan2(__qy, __qx)")
            );
        }

        // --- Per-sample time scoping (see FromPointMovement / MovementPredict) ---
        //
        // Variable layering convention inside a FromPointMovement point subgraph:
        //   __x, __y   : current movement output point (established convention)
        //   __t        : current sample time; rebound by shadowing (`local __t = ...`)
        //                inside do-blocks to sample movements at remapped times
        //   __val      : scalar product (established convention)
        //   __valx/__valy : Vector2 product (established convention)
        // FromPointMovement expands its point subgraph once per frame with the
        // ambient __t, so MovementTransformInputTime inside it reads the current
        // sample time directly; MovementPredict rebinds __t to sample a wired
        // movement at any t'. Together they form the unified custom-transform
        // path Movement-Predict-FromPointMovement.

        /// <summary>
        /// 子图入口：读取当前采样时间 t（__t），暴露为标量 __val。
        /// 仅在 FromPointMovement 的点子图内有意义；配合 Math 组合子可拼 φ(t)。
        /// </summary>
        public LuaParser MovementTransformInputTime()
        {
            return (inner) => Concat(
                Single("local __val = __t")
            );
        }

        /// <summary>
        /// 运动采样：在指定时间 t' 处对运动子图采样，输出 __valx/__valy。
        /// 在 do 块内以 local __x/__y/__t 遮蔽环境变量后展开运动子图，
        /// 使其在 t' 处求值且不污染外层 __x/__y。
        /// 是统一路径 Movement-Predict-FromPointMovement 的入口。
        /// </summary>
        public LuaParser MovementPredict(LuaParser movement, LuaParser time)
        {
            string tv = GenId("__tp_t_");
            string px = GenId("__tp_x_"), py = GenId("__tp_y_");
            return (inner) => Concat(
                Single($"local {tv}"),
                Single("do"),
                Shift(time(inner), 1),
                Single($"{tv} = __val", 1),
                Single("end"),
                Single($"local {px}, {py}"),
                Single("do"),
                Single("local __x, __y", 1),
                Single($"local __t = {tv}", 1),
                Shift(movement(inner), 1),
                Single($"{px}, {py} = __x, __y", 1),
                Single("end"),
                Single($"local __valx = {px}"),
                Single($"local __valy = {py}")
            );
        }

        /// <summary>
        /// 零点常量：Predict 的 movement 端口未连接时的降级输出（__valx/__valy = 0, 0）。
        /// </summary>
        public LuaParser ZeroVector2()
        {
            return (inner) => Concat(
                Single("local __valx, __valy = 0, 0")
            );
        }

        /// <summary>
        /// 时间缩放：<c>f(t) ↦ f(a·t)</c>。在 do 块内遮蔽 <c>__t = a·__t</c> 后展开运动子图。
        /// </summary>
        public LuaParser MovementScaleTime(LuaParser movement, LuaParser factor)
        {
            string a = GenId("__st_a_");
            return (inner) => Concat(
                Single($"local {a}"),
                Single("do"),
                Shift(factor(inner), 1),
                Single($"{a} = __val", 1),
                Single("end"),
                Single("do"),
                Single($"local __t = {a} * __t", 1),
                Shift(movement(inner), 1),
                Single("end")
            );
        }

        /// <summary>
        /// 时间平移：<c>f(t) ↦ f(t+Δ)</c>。在 do 块内遮蔽 <c>__t = __t+Δ</c> 后展开运动子图。
        /// </summary>
        public LuaParser MovementShiftTime(LuaParser movement, LuaParser delta)
        {
            string d = GenId("__st_d_");
            return (inner) => Concat(
                Single($"local {d}"),
                Single("do"),
                Shift(delta(inner), 1),
                Single($"{d} = __val", 1),
                Single("end"),
                Single("do"),
                Single($"local __t = __t + {d}", 1),
                Shift(movement(inner), 1),
                Single("end")
            );
        }

        /// <summary>
        /// Vector2Split 的 X 分量输出：消费输入 Vector2（__valx/__valy），产出 scalar __val = X。
        /// </summary>
        public LuaParser Vector2SplitX(LuaParser vec)
        {
            string vx = GenId("__vx_"), vy = GenId("__vy_");
            return (inner) => Concat(
                Single($"local {vx}, {vy}"),
                Single("do"),
                Shift(vec(inner), 1),
                Single($"{vx}, {vy} = __valx, __valy", 1),
                Single("end"),
                Single($"local __val = {vx}")
            );
        }

        /// <summary>
        /// Vector2Split 的 Y 分量输出：消费输入 Vector2（__valx/__valy），产出 scalar __val = Y。
        /// </summary>
        public LuaParser Vector2SplitY(LuaParser vec)
        {
            string vx = GenId("__vx_"), vy = GenId("__vy_");
            return (inner) => Concat(
                Single($"local {vx}, {vy}"),
                Single("do"),
                Shift(vec(inner), 1),
                Single($"{vx}, {vy} = __valx, __valy", 1),
                Single("end"),
                Single($"local __val = {vy}")
            );
        }

        public LuaParser SingleDataPattern(LuaParser transformation)
        {
            return (inner) => Concat(
                Single("do"),
                Shift(transformation(inner), 1),
                Shift(inner, 1),
                Single("end")
            );
        }

        public LuaParser SingleIntervalPattern(LuaParser interval)
        {
            return (inner) => Concat(
                Single("local __intv"),
                Single("do"),
                Shift(interval(inner), 1),
                Single("__intv = __val", 1),
                Single("end"),
                Single("__wait(__intv)")
            );
        }

        public LuaParser ConcatPattern(LuaParser p1, LuaParser p2)
        {
            return (inner) => Concat(
                p1(inner),
                p2(inner)
            );
        }

        public LuaParser FilterPattern(LuaParser pattern, LuaParser predicate)
        {
            return (inner) => pattern(
                Concat(
                    Single("do"),
                    Shift(predicate(inner), 1),
                    Single("if __val ~= 0 then", 1),
                    Shift(inner, 2),
                    Single("end", 1),
                    Single("end")
                )
            );
        }

        public LuaParser SkipPattern(LuaParser pattern, LuaParser count)
        {
            return (inner) => Concat(
                Single("local __skip"),
                Single("do"),
                Shift(count(inner), 1),
                Single("__skip = __val", 1),
                Single("end"),
                pattern(
                    Concat(
                        Single("if __skip > 0 then"),
                        Single("__skip = __skip - 1", 1),
                        Single("else"),
                        Shift(inner, 1),
                        Single("end")
                    )
                )
            );
        }

        public LuaParser TakePattern(LuaParser pattern, LuaParser count)
        {
            return (inner) => Concat(
                Single("local __take"),
                Single("do"),
                Shift(count(inner), 1),
                Single("__take = __val", 1),
                Single("end"),
                pattern(
                    Concat(
                        Single("if __take > 0 then"),
                        Single("__take = __take - 1", 1),
                        Shift(inner, 1),
                        Single("else"),
                        Single("break", 1),
                        Single("end")
                    )
                )
            );
        }

        public LuaParser SkipWhilePattern(LuaParser pattern, LuaParser predicate)
        {
            return (inner) => Concat(
                Single("local __sw_skip = true"),
                pattern(
                    Concat(
                        Single("if __sw_skip then"),
                        Single("do", 1),
                        Shift(predicate(inner), 2),
                        Single("if __val ~= 0 then", 2),
                        Single("-- skipping", 3),
                        Single("else", 2),
                        Single("__sw_skip = false", 3),
                        Shift(inner, 3),
                        Single("end", 2),
                        Single("end", 1),
                        Single("else"),
                        Shift(inner, 1),
                        Single("end")
                    )
                )
            );
        }

        public LuaParser TakeWhilePattern(LuaParser pattern, LuaParser predicate)
        {
            return (inner) => Concat(
                Single("local __tw_take = true"),
                pattern(
                    Concat(
                        Single("if __tw_take then"),
                        Single("do", 1),
                        Shift(predicate(inner), 2),
                        Single("if __val ~= 0 then", 2),
                        Shift(inner, 3),
                        Single("else", 2),
                        Single("__tw_take = false", 3),
                        Single("break", 3),
                        Single("end", 2),
                        Single("end", 1),
                        Single("end")
                    )
                )
            );
        }

        public LuaParser ReversePattern(LuaParser pattern)
        {
            return (inner) => Concat(
                Single("do"),
                Single("local __rev_buf = {}", 1),
                Single("local __rev_co = {}", 1),

                Single("local __rev_orig_wait = __wait", 1),
                Single("local __rev_orig_new_task = __new_task", 1),
                Single("local __rev_orig_create_and_attach_movement = __create_and_attach_movement", 1),

                Single("__create_and_attach_movement = function(fn) table.insert(__rev_buf, fn) end", 1),
                Single("__new_task = function(fn) table.insert(__rev_co, coroutine.create(fn)) end", 1),
                Single("__wait = function(n) for i = 1, n do coroutine.yield() end end", 1),

                Single("__new_task(function()", 1),
                Shift(pattern(inner), 2),
                Single("end)", 1),

                Single("local __status = true", 1),
                Single("while __status do", 1),
                Single("__status = false", 2),
                Single("for i = 1, #__rev_co do", 2),
                Single("if coroutine.status(__rev_co[i]) ~= 'dead' then", 3),
                Single("__status = true", 4),
                Single("local __ok, __err = coroutine.resume(__rev_co[i])", 4),
                Single("if not __ok then error(__err) end", 4),
                Single("end", 3),
                Single("end", 2),
                Single("if __status then", 2),
                Single("if type(__rev_buf[#__rev_buf]) == 'number' then", 3),
                Single("__rev_buf[#__rev_buf] = __rev_buf[#__rev_buf] + 1", 4),
                Single("else", 3),
                Single("__rev_buf[#__rev_buf + 1] = 1", 4),
                Single("end", 3),
                Single("end", 2),
                Single("end", 1),

                Single("__wait = __rev_orig_wait", 1),
                Single("__new_task = __rev_orig_new_task", 1),
                Single("__create_and_attach_movement = __rev_orig_create_and_attach_movement", 1),

                Single("for __ri = #__rev_buf, 1, -1 do", 1),
                Single("local __e = __rev_buf[__ri]", 2),
                Single("if type(__e) == 'number' then", 2),
                Single("__wait(__e)", 3),
                Single("elseif type(__e) == 'function' then", 2),
                Single("__create_and_attach_movement(__e)", 3),
                Single("end", 2),
                Single("end", 1),
                Single("end")
            );
        }

        public LuaParser TrimStartPattern(LuaParser pattern)
        {
            return (inner) => Concat(
                Single("do"),
                Single("local __ts_started = false", 1),
                Single("local __ts_orig_wait = __wait", 1),
                Single("local __wait = function(n) if __ts_started then __ts_orig_wait(n) end end", 1),
                Shift(
                    pattern(
                        Concat(
                            Single("__ts_started = true"),
                            inner
                        )
                    ),
                    1
                ),
                Single("end")
            );
        }

        public LuaParser TrimEndPattern(LuaParser pattern)
        {
            return (inner) => Concat(
                Single("do"),
                Single("local __te_pending = 0", 1),
                Single("local __te_orig_wait = __wait", 1),
                Single("local __wait = function(n) __te_pending = __te_pending + n end", 1),
                Shift(
                    pattern(
                        Concat(
                            Single("__te_orig_wait(__te_pending)"),
                            Single("__te_pending = 0"),
                            inner
                        )
                    ),
                    1
                ),
                Single("end")
            );
        }

        public LuaParser TrimPattern(LuaParser pattern)
        {
            return (inner) => Concat(
                Single("do"),
                Single("local __tr_started = false", 1),
                Single("local __tr_pending = 0", 1),
                Single("local __tr_orig_wait = __wait", 1),
                Single("local __wait = function(n) __tr_pending = __tr_pending + n end", 1),
                Shift(
                    pattern(
                        Concat(
                            Single("if __tr_started then"),
                            Single("__tr_orig_wait(__tr_pending)", 1),
                            Single("end"),
                            Single("__tr_pending = 0"),
                            Single("__tr_started = true"),
                            inner
                        )
                    ),
                    1
                ),
                Single("end")
            );
        }

        public LuaParser Unknown()
        {
            return (inner) => Concat(
                Single("--[[ Unknown node type ]]"),
                inner
            );
        }

        public LuaParser Empty()
        {
            return _ => System.Linq.Enumerable.Empty<LuaCodeLine>();
        }

        private IEnumerable<LuaCodeLine> Single(string text, int indent = 0)
        {
            yield return new LuaCodeLine(text, indent);
        }

        private IEnumerable<LuaCodeLine> Concat(params IEnumerable<LuaCodeLine>[] sources)
        {
            foreach (var source in sources)
                foreach (var line in source)
                    yield return line;
        }

        private IEnumerable<LuaCodeLine> Shift(IEnumerable<LuaCodeLine> lines, int delta)
        {
            foreach (var line in lines)
                yield return line with { Indent = line.Indent + delta };
        }

        private string FlatText(IEnumerable<LuaCodeLine> lines)
            => string.Join("", lines.Select(l => l.Text));
    }
}
