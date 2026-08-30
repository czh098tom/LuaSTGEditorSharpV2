using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class ComponentParserTests
{
    [Fact]
    public void Parse_Empty_ReturnsEmpty()
    {
        var fragments = FragmentParser.Parse("");
        var result = ComponentParser.Parse(fragments);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_SingleComponent_ReturnsOneComponent()
    {
        var fragments = FragmentParser.Parse("abc");
        var result = ComponentParser.Parse(fragments);
        result.Should().HaveCount(1);
        ToString(result[0]).Should().Be("abc");
    }

    [Fact]
    public void Parse_MultipleComponents_SplitsByComma()
    {
        var fragments = FragmentParser.Parse("a, b, c");
        var result = ComponentParser.Parse(fragments);
        result.Should().HaveCount(3);
        ToString(result[0]).Should().Be("a");
        ToString(result[1]).Should().Be("b");
        ToString(result[2]).Should().Be("c");
    }

    [Fact]
    public void Parse_TrimsWhitespace()
    {
        var fragments = FragmentParser.Parse("  a  ,   b   ");
        var result = ComponentParser.Parse(fragments);
        result.Should().HaveCount(2);
        ToString(result[0]).Should().Be("a");
        ToString(result[1]).Should().Be("b");
    }

    [Fact]
    public void Parse_CommaInsideParentheses_NotSplit()
    {
        var fragments = FragmentParser.Parse("a, (b, c), d");
        var result = ComponentParser.Parse(fragments);
        result.Should().HaveCount(3);
        ToString(result[0]).Should().Be("a");
        ToString(result[1]).Should().Be("(b, c)");
        ToString(result[2]).Should().Be("d");
    }

    [Fact]
    public void Parse_NestedParentheses_HandlesCorrectly()
    {
        var fragments = FragmentParser.Parse("a, ((b, c), d)");
        var result = ComponentParser.Parse(fragments);
        result.Should().HaveCount(2);
        ToString(result[0]).Should().Be("a");
        ToString(result[1]).Should().Be("((b, c), d)");
    }

    [Fact]
    public void Parse_CommaAtDifferentDepths_OnlyDepth0Splits()
    {
        var fragments = FragmentParser.Parse("a, (b, (c, d))");
        var result = ComponentParser.Parse(fragments);
        result.Should().HaveCount(2);
        ToString(result[0]).Should().Be("a");
        ToString(result[1]).Should().Be("(b, (c, d))");
    }

    [Fact]
    public void Reconstruct_SingleComponent_NoComma()
    {
        var fragments = FragmentParser.Parse("abc");
        var components = ComponentParser.Parse(fragments);
        var result = ComponentParser.Reconstruct(components);
        FragmentParser.Reconstruct(result).Should().Be("abc");
    }

    [Fact]
    public void Reconstruct_MultipleComponents_AddsCommas()
    {
        var fragments = FragmentParser.Parse("a, b, c");
        var components = ComponentParser.Parse(fragments);
        var result = ComponentParser.Reconstruct(components);
        FragmentParser.Reconstruct(result).Should().Be("a,b,c");
    }

    [Fact]
    public void RoundTrip_ComplexExpression_PreservesContent()
    {
        var original = "a, (b, c), d";
        var fragments = FragmentParser.Parse(original);
        var components = ComponentParser.Parse(fragments);
        var reconstructed = ComponentParser.Reconstruct(components);
        FragmentParser.Reconstruct(reconstructed).Should().Be("a,(b, c),d");
    }

    [Fact]
    public void Reconstruct_WithSpaceAfterComma_AddsSpace()
    {
        var original = ParserOptions.Default;
        try
        {
            ParserOptions.Default = new ParserOptions { SpaceAfterComma = true };
            var components = new[]
            {
                FragmentParser.Parse("a"),
                FragmentParser.Parse("b"),
                FragmentParser.Parse("c")
            };
            var result = ComponentParser.Reconstruct(components);
            FragmentParser.Reconstruct(result).Should().Be("a, b, c");
        }
        finally
        {
            ParserOptions.Default = original;
        }
    }

    private static string ToString(Fragment[] fragments)
        => new string(fragments.Select(f => f.Character).ToArray());
}
