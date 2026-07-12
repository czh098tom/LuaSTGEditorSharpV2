using System.Collections.Generic;
using System.Linq;

namespace LinqSTG.Expression.ToLua
{
    public static class Parser
    {
        public static LuaParser Shoot(LuaParser pattern, LuaParser movement)
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

        public static LuaParser RepeatWithIntervalPattern(LuaParser times, LuaParser interval, LuaParser repeater)
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
                Single("for __i = 1, __t do"),
                Single($"local {FlatText(repeater(inner))} = __t, __i", 1),
                Shift(inner, 1),
                Single("__wait(__intv)", 1),
                Single("end")
            );
        }

        public static LuaParser Repeater(LuaParser curr, LuaParser max)
        {
            return (inner) => Single($"{FlatText(max(inner))}, {FlatText(curr(inner))}");
        }

        public static LuaParser DefaultRepeater()
        {
            return (inner) => Single($"__t, __i");
        }

        public static LuaParser Sample01MinMax(LuaParser repeater, LuaParser lb, LuaParser ub, IntervalType intervalType)
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

        public static LuaParser Sample01(LuaParser repeater, IntervalType intervalType)
        {
            return (inner) => Concat(
                Single($"local __max, __curr = {FlatText(repeater(inner))}"),
                GetIntervalManipulater("__val", intervalType)(inner)
            );
        }

        private static LuaParser GetIntervalManipulater(string name, IntervalType intervalType)
        {
            return intervalType switch
            {
                IntervalType.Open => (inner) => Single($"local {name} = __curr / (__max + 1)"),
                IntervalType.HeadClosed => (inner) => Single($"local {name} = (__curr - 1) / __max"),
                IntervalType.TailClosed => (inner) => Single($"local {name} = __curr / __max"),
                IntervalType.BothClosed => (inner) => Single($"local {name} = (__curr - 1) / (__max - 1)"),
                _ => GetIntervalManipulater(name, IntervalType.HeadClosed)
            };
        }

        public static LuaParser MinMax(LuaParser value, LuaParser lb, LuaParser ub)
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

        public static LuaParser RepeatPattern(LuaParser times, LuaParser repeaterKey)
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
                    Single("for __i = 1, __t do"),
                    Single($"local {repeaterText} = __t, __i", 1),
                    Shift(inner, 1),
                    Single("end")
                );
            };
        }

        public static LuaParser TakeRepeaterFromContext(LuaParser repeaterKey)
        {
            return repeaterKey;
        }

        public static LuaParser ConstantFloat(float value)
        {
            return _ => Single($"local __val = {value}");
        }

        public static LuaParser ConstantString(string str)
        {
            return _ => Single(str);
        }

        public static LuaParser IntrinsicAdd(LuaParser lhs, LuaParser rhs)
        {
            return (inner) => Concat(
                Single("local __lhs, __rhs"),
                Single("do"),
                Shift(lhs(inner), 1),
                Single("__lhs = __val", 1),
                Single("end"),
                Single("do"),
                Shift(rhs(inner), 1),
                Single("__rhs = __val", 1),
                Single("end"),
                Single("local __val = __lhs + __rhs")
            );
        }

        public static LuaParser UniformVelocityMovement(LuaParser vec)
        {
            return (inner) => Concat(
                Single("local __vx, __vy"),
                Single("do"),
                Shift(vec(inner), 1),
                Single("__vx, __vy = __valx, __valy", 1),
                Single("end"),
                Single("__x = __vx * __t"),
                Single("__y = __vy * __t")
            );
        }

        public static LuaParser VectorFromAngleLength(LuaParser angle, LuaParser length)
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

        public static LuaParser TakeVariableFromContext(LuaParser key)
        {
            return (inner) => Single($"local __val = {FlatText(key(inner))}");
        }

        public static LuaParser MovementAfterTime(LuaParser m1, LuaParser time, LuaParser m2)
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

        public static LuaParser Assign(LuaParser prev, LuaParser value, LuaParser key)
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

        public static LuaParser MapPattern(LuaParser pattern, LuaParser transformation)
        {
            return (inner) => pattern(Concat(transformation(inner), inner));
        }

        public static LuaParser ExtrudePattern(LuaParser pattern, LuaParser transformation)
        {
            return (inner) => pattern(Concat(
                Single("__new_task(function(self)"),
                Shift(transformation(inner), 1),
                Single("end)")
            ));
        }

        public static LuaParser ExtrudeConcatPattern(LuaParser pattern, LuaParser subPattern)
        {
            return (inner) => pattern(Concat(
                Single("do"),
                Shift(subPattern(inner), 1),
                Single("end")
            ));
        }

        public static LuaParser Vector2(LuaParser x, LuaParser y)
        {
            return (inner) => Concat(
                Single("local __vx, __vy"),
                Single("do"),
                Shift(x(inner), 1),
                Single("__vx = __val", 1),
                Single("end"),
                Single("do"),
                Shift(y(inner), 1),
                Single("__vy = __val", 1),
                Single("end"),
                Single("local __valx = __vx"),
                Single("local __valy = __vy")
            );
        }

        public static LuaParser FloatToInt(LuaParser f)
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

        public static LuaParser IntToFloat(LuaParser f)
        {
            return f;
        }

        public static LuaParser StationaryMovement(LuaParser position)
        {
            return (inner) => Concat(
                Single("do"),
                Shift(position(inner), 1),
                Single("__x = __valx", 1),
                Single("__y = __valy", 1),
                Single("end")
            );
        }

        public static LuaParser UniformAccelerationMovement(LuaParser initialVelocity, LuaParser acceleration)
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

        public static LuaParser MovementSum(LuaParser m1, LuaParser m2)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(m1(inner), 1),
                Single("__sx, __sy = __x, __y", 1),
                Single("end"),
                Single("do"),
                Shift(m2(inner), 1),
                Single("__sx = __sx + __x", 1),
                Single("__sy = __sy + __y", 1),
                Single("end"),
                Single("__x = __sx"),
                Single("__y = __sy")
            );
        }

        public static LuaParser MovementOffset(LuaParser movement, LuaParser offset)
        {
            return (inner) => Concat(
                Single("local __sx, __sy"),
                Single("do"),
                Shift(movement(inner), 1),
                Single("__sx, __sy = __x, __y", 1),
                Single("end"),
                Single("do"),
                Shift(offset(inner), 1),
                Single("__sx = __sx + __valx", 1),
                Single("__sy = __sy + __valy", 1),
                Single("end"),
                Single("__x = __sx"),
                Single("__y = __sy")
            );
        }

        public static LuaParser SingleDataPattern(LuaParser transformation)
        {
            return (inner) => Concat(
                Single("do"),
                Shift(transformation(inner), 1),
                Shift(inner, 1),
                Single("end")
            );
        }

        public static LuaParser SingleIntervalPattern(LuaParser interval)
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

        public static LuaParser ConcatPattern(LuaParser p1, LuaParser p2)
        {
            return (inner) => Concat(
                p1(inner),
                p2(inner)
            );
        }

        public static LuaParser FilterPattern(LuaParser pattern, LuaParser predicate)
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

        public static LuaParser SkipPattern(LuaParser pattern, LuaParser count)
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

        public static LuaParser TakePattern(LuaParser pattern, LuaParser count)
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

        public static LuaParser SkipWhilePattern(LuaParser pattern, LuaParser predicate)
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

        public static LuaParser TakeWhilePattern(LuaParser pattern, LuaParser predicate)
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

        public static LuaParser ReversePattern(LuaParser pattern)
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

        public static LuaParser TrimStartPattern(LuaParser pattern)
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

        public static LuaParser TrimEndPattern(LuaParser pattern)
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

        public static LuaParser TrimPattern(LuaParser pattern)
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

        public static LuaParser Unknown()
        {
            return (inner) => Concat(
                Single("--[[ Unknown node type ]]"),
                inner
            );
        }

        public static LuaParser Empty()
        {
            return _ => System.Linq.Enumerable.Empty<LuaCodeLine>();
        }

        private static IEnumerable<LuaCodeLine> Single(string text, int indent = 0)
        {
            yield return new LuaCodeLine(text, indent);
        }

        private static IEnumerable<LuaCodeLine> Concat(params IEnumerable<LuaCodeLine>[] sources)
        {
            foreach (var source in sources)
                foreach (var line in source)
                    yield return line;
        }

        private static IEnumerable<LuaCodeLine> Shift(IEnumerable<LuaCodeLine> lines, int delta)
        {
            foreach (var line in lines)
                yield return line with { Indent = line.Indent + delta };
        }

        private static string FlatText(IEnumerable<LuaCodeLine> lines)
            => string.Join("", lines.Select(l => l.Text));
    }
}
