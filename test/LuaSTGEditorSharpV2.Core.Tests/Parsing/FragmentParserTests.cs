using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class FragmentParserTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsEmptyArray()
    {
        var result = FragmentParser.Parse("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NoParentheses_AllDepthZero()
    {
        var result = FragmentParser.Parse("abc");
        GetDepths(result).Should().Be("000");
    }

    [Fact]
    public void Parse_SingleParentheses_TracksDepthCorrectly()
    {
        var result = FragmentParser.Parse("(a)");
        GetDepths(result).Should().Be("010");
    }

    [Fact]
    public void Parse_NestedParentheses_TracksDepthCorrectly()
    {
        var result = FragmentParser.Parse("((a))");
        GetDepths(result).Should().Be("01210");
    }

    [Fact]
    public void Parse_ComplexExpression_TracksDepthCorrectly()
    {
        var input = "a + (b - c), d(e)";
        var expected = "00000111110000010";
        var result = FragmentParser.Parse(input);
        GetDepths(result).Should().Be(expected);
    }

    [Fact]
    public void Parse_DeepNesting_TracksCorrectly()
    {
        var result = FragmentParser.Parse("(((a)))");
        GetDepths(result).Should().Be("0123210");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("(a)")]
    [InlineData("((a)+(b))")]
    public void TryParse_BalancedInput_ReturnsFragments(string input)
    {
        var parsed = FragmentParser.TryParse(input, out var result);

        parsed.Should().BeTrue();
        FragmentParser.Reconstruct(result).Should().Be(input);
    }

    [Theory]
    [InlineData(")")]
    [InlineData("a)")]
    [InlineData("(a")]
    [InlineData("((a)")]
    public void TryParse_UnbalancedInput_ReturnsFalse(string input)
    {
        var parsed = FragmentParser.TryParse(input, out var result);

        parsed.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Reconstruct_RoundTrip_ReturnsOriginalString()
    {
        var original = "a + (b - c), d";
        var fragments = FragmentParser.Parse(original);
        var result = FragmentParser.Reconstruct(fragments);
        result.Should().Be(original);
    }

    private static string GetDepths(Fragment[] fragments)
        => new string(fragments.Select(f => (char)('0' + f.Depth)).ToArray());
}
