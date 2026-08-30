using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Tests;

public class CodeDataTests
{
    [Fact]
    public void LineCount_CountsNewlines()
    {
        var data = new CodeData("a\nb\nc", NodeData.Empty);

        Assert.Equal(2, data.LineCount);
        Assert.Equal("a\nb\nc", data.Content);
    }

    [Fact]
    public void EmptyContent_LineCountZero()
    {
        var data = new CodeData("", NodeData.Empty);

        Assert.Equal(0, data.LineCount);
    }

    [Fact]
    public void NoNewline_LineCountZero()
    {
        var data = new CodeData("single line", NodeData.Empty);

        Assert.Equal(0, data.LineCount);
    }

    [Fact]
    public void Constructor_PreservesSource()
    {
        var source = new NodeData("TestType");
        var data = new CodeData("code", source);

        Assert.Same(source, data.Source);
    }
}
