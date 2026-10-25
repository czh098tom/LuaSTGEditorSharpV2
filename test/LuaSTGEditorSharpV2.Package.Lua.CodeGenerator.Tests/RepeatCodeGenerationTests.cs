using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;
using Xunit;
using Fixture = LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Tests.AdvancedRepeatCodeGenerationTests.Fixture;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator.Tests;

public sealed class RepeatCodeGenerationTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public void ActiveVariableRowsChooseTheExistingLayout(int count, bool hasVariables)
    {
        using var fixture = new Fixture();
        var repeat = Node("Repeat", ("times", "4"), ("count", count.ToString()));
        for (var i = 0; i < count; i++) repeat.Properties[$"name_{i}"] = " \t";
        if (hasVariables)
        {
            repeat.Properties["name_0"] = "counter";
            repeat.Properties["init_0"] = "2";
            repeat.Properties["increment_0"] = "3";
            repeat.Properties["name_2"] = "position";
        }
        var marker = Node("marker");
        repeat.Add(marker);
        var output = fixture.Generate(repeat);

        var expected = hasVariables
            ? "do\n local counter, _d_counter = (2), (3)\n" +
                " local position, _d_position = (0), (0)\n for _ = 1, 4 do\n" +
                "  marker\n  counter = counter + _d_counter\n" +
                "  position = position + _d_position\n end\nend\n"
            : "for _ = 1, 4 do\n marker\nend\n";
        Assert.Equal(expected, string.Concat(output.Select(code => code.Content)));
        Assert.Equal(hasVariables
            ? new[] { repeat, repeat, repeat, repeat, marker, repeat, repeat, repeat, repeat }
            : new[] { repeat, marker, repeat }, SourcesByLine(output));
    }

    [Fact]
    public void NestedLoopsKeepTheirOwnCapturesAndSources()
    {
        using var fixture = new Fixture();
        var outer = Node("Repeat", ("times", "2"), ("count", "1"),
            ("name_0", "counter"), ("init_0", "2"), ("increment_0", "3"));
        var inner = Node("Repeat", ("times", "3"), ("count", "2"));
        var marker = Node("marker");
        var disabled = Node("marker");
        disabled.IsActive = false;
        inner.Add(marker);
        inner.Add(disabled);
        outer.Add(inner);
        var output = fixture.Generate(outer);

        Assert.Equal("do\n local counter, _d_counter = (2), (3)\n for _ = 1, 2 do\n" +
            "  for _ = 1, 3 do\n   marker\n  end\n" +
            "  counter = counter + _d_counter\n end\nend\n",
            string.Concat(output.Select(code => code.Content)));
        Assert.Equal(new[] { outer, outer, outer, inner, marker, inner, outer, outer, outer },
            SourcesByLine(output));
        Assert.Null(fixture.Context.Peek());
    }

    [Fact]
    public void TimesExpandMacrosWhileVariableRowsKeepRawValues()
    {
        using var fixture = new Fixture();
        var macro = Node("macro", ("replace", "COUNT"), ("by", "COUNT+1"));
        var repeat = Node("Repeat", ("times", "COUNT"), ("count", "1"),
            ("name_0", "COUNT"), ("init_0", "COUNT"), ("increment_0", "COUNT"));
        repeat.Add(Node("marker"));
        macro.Add(repeat);

        Assert.Equal("do\n local COUNT, _d_COUNT = (COUNT), (COUNT)\n" +
            " for _ = 1, COUNT+1 do\n  marker\n  COUNT = COUNT + _d_COUNT\n end\nend\n",
            string.Concat(fixture.Generate(macro).Select(code => code.Content)));
    }

    private static NodeData[] SourcesByLine(IEnumerable<CodeData> output) =>
        output.SelectMany(code => Enumerable.Repeat(code.Source, code.LineCount)).ToArray();

    private static NodeData Node(string type, params (string Key, string Value)[] values)
    {
        var node = new NodeData(type);
        foreach (var (key, value) in values) node.Properties[key] = value;
        return node;
    }
}
