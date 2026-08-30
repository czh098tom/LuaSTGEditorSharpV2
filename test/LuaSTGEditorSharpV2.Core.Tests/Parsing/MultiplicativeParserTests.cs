using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class MultiplicativeParserTests
{
    [Fact]
    public void Parse_Empty_ReturnsEmpty()
    {
        var result = MultiplicativeParser.Parse(FragmentParser.Parse(string.Empty));

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_TopLevelProducts_ReturnsTrimmedFactors()
    {
        var result = MultiplicativeParser.Parse(FragmentParser.Parse(" radius * cos(angle) "));

        result.Select(FragmentParser.Reconstruct).Should().Equal("radius", "cos(angle)");
    }

    [Fact]
    public void Parse_NestedProducts_DoesNotSplitNestedFactors()
    {
        var result = MultiplicativeParser.Parse(FragmentParser.Parse("(a*b)*cos(c*d)"));

        result.Select(FragmentParser.Reconstruct).Should().Equal("(a*b)", "cos(c*d)");
    }

    [Theory]
    [InlineData("*a")]
    [InlineData("a*")]
    [InlineData("a**b")]
    public void Parse_EmptyFactor_PreservesInvalidSegment(string value)
    {
        var result = MultiplicativeParser.Parse(FragmentParser.Parse(value));

        result.Should().Contain(factor => factor.Length == 0);
    }

    [Fact]
    public void Reconstruct_Product_JoinsFactors()
    {
        var factors = new[]
        {
            FragmentParser.Parse("radius"),
            FragmentParser.Parse("cos(angle)"),
        };

        var result = MultiplicativeParser.Reconstruct(factors);

        FragmentParser.Reconstruct(result).Should().Be("radius*cos(angle)");
    }
}
