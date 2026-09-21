using System.Diagnostics;
using Xunit;

namespace LinqSTG.Expression.ToLua.Tests
{
    public sealed class LuaFactAttribute : FactAttribute
    {
        public LuaFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LINQSTG_LUA_EXECUTABLE")))
                Skip = "Set LINQSTG_LUA_EXECUTABLE to run generated Lua against a real interpreter.";
        }
    }

    public class NumericLuaRuntimeTests
    {
        private static string Emit(LuaParser parser) =>
            string.Join("\n", parser([]).Select(line => new string(' ', line.Indent * 2) + line.Text));

        private static void Run(string source)
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("LINQSTG_LUA_EXECUTABLE")!,
                Arguments = "-",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            })!;
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            process.StandardInput.Write(source);
            process.StandardInput.Close();
            if (!process.WaitForExit(10000))
            {
                process.Kill();
                throw new TimeoutException("Generated numeric Lua did not finish.");
            }
            Assert.True(process.ExitCode == 0, error.GetAwaiter().GetResult() + output.GetAwaiter().GetResult());
        }

        [LuaFact]
        public void FloatToInt_RejectsInvalidValuesAndRoundsTiesToEven()
        {
            var parser = new Parser();
            LuaParser input = _ => [new LuaCodeLine("local __val = input", 0)];
            var body = Emit(parser.FloatToInt(input));
            Run("local function convert(input)\n" + body + "\nreturn __val end\n" + """
                for _, pair in ipairs({
                    {2.5, 2}, {3.5, 4}, {-2.5, -2}, {-3.5, -4},
                    {2.6, 3}, {-2.6, -3}, {2147483520, 2147483520},
                    {-2147483648, -2147483648}
                }) do assert(convert(pair[1]) == pair[2]) end
                for _, value in ipairs({0/0, math.huge, -math.huge, 2147483648, -2147483904}) do
                    assert(not pcall(convert, value))
                end
                """);
        }

        [LuaFact]
        public void NestedConversions_EvaluateTheInputOnce()
        {
            var parser = new Parser();
            LuaParser input = _ => [
                new LuaCodeLine("calls = calls + 1", 0),
                new LuaCodeLine("local __val = 2.5", 0)
            ];
            Run("local calls = 0\n" + Emit(parser.FloatToInt(parser.IntToFloat(parser.FloatToInt(input))))
                + "\nassert(__val == 2 and calls == 1)");
        }

        [LuaFact]
        public void SamplingAndRemapping_HandleMissingAndSingleRepeaters()
        {
            foreach (var interval in Enum.GetValues<IntervalType>())
            {
                var parser = new Parser();
                LuaParser repeater = _ => [new LuaCodeLine("total, id", 0)];
                var sample = Emit(parser.Sample01(repeater, interval));
                Run("local function sample(total, id)\n" + sample
                    + "\nreturn __val end\nassert(sample(0, 0) == 0)\nassert(sample(-1, 0) == 0)");
            }

            var closed = new Parser();
            LuaParser single = _ => [new LuaCodeLine("1, 0", 0)];
            Run(Emit(closed.Sample01MinMax(single, closed.ConstantFloat(3), closed.ConstantFloat(9),
                IntervalType.BothClosed)) + "\nassert(__val == 3)");
            LuaParser lower = _ => [new LuaCodeLine("local __valx, __valy = 3, 4", 0)];
            LuaParser upper = _ => [new LuaCodeLine("local __valx, __valy = 9, 12", 0)];
            Run(Emit(closed.Sample01MinMaxVector2(single, lower, upper, IntervalType.BothClosed))
                + "\nassert(__valx == 3 and __valy == 4)");
        }

        [LuaFact]
        public void RepeaterRead_UsesIntegerConversion()
        {
            var parser = new Parser();
            LuaParser keys = _ => [new LuaCodeLine("total, id", 0)];
            var expression = Emit(parser.TakeRepeaterFromContext(keys));
            Run("local function read(total, id)\nreturn " + expression + "\nend\n"
                + "local total, id = read(3.5, 2.5)\nassert(total == 4 and id == 2)\n"
                + "assert(not pcall(read, 0/0, 1))");
        }
    }
}
