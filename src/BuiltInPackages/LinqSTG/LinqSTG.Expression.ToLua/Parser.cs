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
                    inner,
                    Single("local obj = last"),
                    Single("task.New(obj, function(self)"),
                    Single("local __t", 1),
                    Single("for _ = 1, _infinite do", 1),
                    Single("__t = self.timer", 2),
                    Single("local __x, __y = 0, 0", 2),
                    Shift(movement(inner), 2),
                    Single("self.x = __x", 2),
                    Single("self.y = __y", 2),
                    Single("end", 1),
                    Single("end)")
                );
                return Concat(
                    Single("local __new_task = function(fn) task.New(self, fn) end"),
                    Single("local __wait = task.Wait"),
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

        public static LuaParser Sample01MinMax(LuaParser repeater, LuaParser lb, LuaParser ub)
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
                Single("local __val = __curr / __max * (__ub - __lb) + __lb")
            );
        }

        public static LuaParser Sample01(LuaParser repeater)
        {
            return (inner) => Concat(
                Single($"local __max, __curr = {FlatText(repeater(inner))}"),
                Single("local __val = __curr / __max")
            );
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
                Single("do"),
                Shift(vec(inner), 1),
                Single("local __vx, __vy = __valx, __valy", 1),
                Single("__x = __vx * __t", 1),
                Single("__y = __vy * __t", 1),
                Single("end")
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
                Single("if last.timer < __ts then"),
                Shift(m1(inner), 1),
                Single("else"),
                Single("local __t2 = __t", 1),
                Single("local __t = __ts", 1),
                Shift(m1(inner), 1),
                Single("local __t = __t2 - __ts", 1),
                Single("local __x1, __y1 = __x, __y", 1),
                Shift(m2(inner), 1),
                Single("__x = __x1 + __x", 1),
                Single("__y = __y1 + __y", 1),
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
                Single("end)"),
                inner
            ));
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
