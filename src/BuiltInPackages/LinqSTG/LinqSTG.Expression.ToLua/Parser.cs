using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqSTG.Expression.ToLua
{
    public static class Parser
    {
        public static LuaParser Shoot(LuaParser pattern, LuaParser movement)
        {
            return (inner) => $"""
            local __new_task = function(fn) task.New(self, fn) end
            local __wait = task.Wait
            {pattern($"""
                {inner}
                local obj = last
                task.New(obj, function(self) 
                    local __t
                    for _ = 1, _infinite do 
                        __t = self.timer
                        local __x, __y = 0, 0
                        {movement(inner)}
                        self.x = __x
                        self.y = __y
                    end 
                end)
            """)}
            """;
        }

        public static LuaParser RepeatWithIntervalPattern(LuaParser times, LuaParser interval)
        {
            return (inner) => $"""
            local __t = {times(inner)}
            local __intv = {interval(inner)}
            for i = 1, __t do
                {inner}
                __wait(__intv)
            end
            """;
        }

        public static LuaParser ConstantFloat(float value)
        {
            return (inner) => value.ToString();
        }

        public static LuaParser IntrinsicAdd(LuaParser lhs, LuaParser rhs)
        {
            return (inner) => $"""
            {lhs(inner)} + {rhs(inner)}
            """;
        }

        public static LuaParser UniformVelocityMovement(LuaParser vec)
        {
            return (inner) => $"""
            do
                local __vx, __vy = {vec(inner)}
                __x = __vx * __t
                __y = __vy * __t
            end
            """;
        }

        public static LuaParser MovementAfterTime(LuaParser m1, LuaParser time, LuaParser m2)
        {
            return (inner) => $"""
            local __ts = {time(inner)}
            if last.timer < __ts then
                {m1(inner)}
            else
                local __t2 = __t
                local __t = __ts
                {m1(inner)}
                local __t = __t2 - __ts
                local __x1, __y1 = __x, __y
                {m2(inner)}
                __x = __x1 + __x
                __y = __y1 + __y
            end
            """;
        }

        public static LuaParser Assign(LuaParser prev, LuaParser value, LuaParser key)
        {
            return (inner) => $"""
            {prev(inner)}
            local {key(inner)} = {value(inner)}
            """;
        }

        public static LuaParser MapPattern(LuaParser pattern, LuaParser transformation)
        {
            return (inner) => pattern($"""
            {transformation(inner)}
            {inner}
            """);
        }

        public static LuaParser ExtrudePattern(LuaParser pattern, LuaParser transformation)
        {
            return (inner) => pattern($"""
            __new_task(function(self)
                {transformation(inner)}
            end)
            """);
        }

        public static LuaParser Unknown()
        {
            return (inner) => $"""
            --[[ Unknown node type ]]
            {inner}
            """;
        }
    }
}
