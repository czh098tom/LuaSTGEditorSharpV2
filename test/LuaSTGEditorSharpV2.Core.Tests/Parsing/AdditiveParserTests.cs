using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class AdditiveParserTests
{
    [Fact]
    public void Parse_Empty_ReturnsEmpty()
    {
        var fragments = FragmentParser.Parse("");
        var result = AdditiveParser.Parse(fragments);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_SinglePositiveTerm_ReturnsOnePositiveTerm()
    {
        var fragments = FragmentParser.Parse("abc");
        var result = AdditiveParser.Parse(fragments);
        result.Should().HaveCount(1);
        result[0].Sign.Should().Be(Sign.Positive);
        ToString(result[0].Term).Should().Be("abc");
    }

    [Fact]
    public void Parse_TwoTermsWithPlus_ReturnsTwoPositiveTerms()
    {
        var fragments = FragmentParser.Parse("a + b");
        var result = AdditiveParser.Parse(fragments);
        result.Should().HaveCount(2);
        result[0].Sign.Should().Be(Sign.Positive);
        result[1].Sign.Should().Be(Sign.Positive);
        ToString(result[0].Term).Should().Be("a");
        ToString(result[1].Term).Should().Be("b");
    }

    [Fact]
    public void Parse_WithMinus_ReturnsNegativeTerm()
    {
        var fragments = FragmentParser.Parse("a - b");
        var result = AdditiveParser.Parse(fragments);
        result.Should().HaveCount(2);
        result[0].Sign.Should().Be(Sign.Positive);
        result[1].Sign.Should().Be(Sign.Negative);
        ToString(result[0].Term).Should().Be("a");
        ToString(result[1].Term).Should().Be("b");
    }

    [Fact]
    public void Parse_MixedOperators_ReturnsCorrectSigns()
    {
        var fragments = FragmentParser.Parse("a + b - c + d");
        var result = AdditiveParser.Parse(fragments);
        result.Should().HaveCount(4);
        result[0].Sign.Should().Be(Sign.Positive);
        result[1].Sign.Should().Be(Sign.Positive);
        result[2].Sign.Should().Be(Sign.Negative);
        result[3].Sign.Should().Be(Sign.Positive);
    }

    [Fact]
    public void Parse_OperatorInsideParentheses_NotSplit()
    {
        var fragments = FragmentParser.Parse("a + (b - c)");
        var result = AdditiveParser.Parse(fragments);
        result.Should().HaveCount(2);
        ToString(result[0].Term).Should().Be("a");
        ToString(result[1].Term).Should().Be("(b - c)");
    }

    [Fact]
    public void Parse_OperatorAtDifferentDepths_OnlyDepth0Splits()
    {
        var fragments = FragmentParser.Parse("(a + b) - c");
        var result = AdditiveParser.Parse(fragments);
        result.Should().HaveCount(2);
        ToString(result[0].Term).Should().Be("(a + b)");
        result[0].Sign.Should().Be(Sign.Positive);
        ToString(result[1].Term).Should().Be("c");
        result[1].Sign.Should().Be(Sign.Negative);
    }

    [Fact]
    public void Parse_TrimsWhitespace()
    {
        var fragments = FragmentParser.Parse("  a  +   b  ");
        var result = AdditiveParser.Parse(fragments);
        ToString(result[0].Term).Should().Be("a");
        ToString(result[1].Term).Should().Be("b");
    }

    [Fact]
    public void Reconstruct_SinglePositiveTerm_NoOperator()
    {
        var fragments = FragmentParser.Parse("abc");
        var terms = AdditiveParser.Parse(fragments);
        var result = AdditiveParser.Reconstruct(terms);
        FragmentParser.Reconstruct(result).Should().Be("abc");
    }

    [Fact]
    public void Reconstruct_MultipleTerms_AddsOperators()
    {
        var terms = new List<SignedTerm>
        {
            new(Sign.Positive, FragmentParser.Parse("a")),
            new(Sign.Negative, FragmentParser.Parse("b")),
            new(Sign.Positive, FragmentParser.Parse("c"))
        };
        var result = AdditiveParser.Reconstruct(terms);
        FragmentParser.Reconstruct(result).Should().Be("a-b+c");
    }

    [Fact]
    public void Reconstruct_FirstTermNegative_PrependsMinus()
    {
        var terms = new List<SignedTerm>
        {
            new(Sign.Negative, FragmentParser.Parse("a")),
            new(Sign.Positive, FragmentParser.Parse("b"))
        };
        var result = AdditiveParser.Reconstruct(terms);
        FragmentParser.Reconstruct(result).Should().Be("-a+b");
    }

    [Fact]
    public void Reconstruct_FirstTermPositive_NoPrependedPlus()
    {
        var terms = new List<SignedTerm>
        {
            new(Sign.Positive, FragmentParser.Parse("a")),
            new(Sign.Positive, FragmentParser.Parse("b"))
        };
        var result = AdditiveParser.Reconstruct(terms);
        FragmentParser.Reconstruct(result).Should().Be("a+b");
    }

    [Fact]
    public void RoundTrip_MixedOperators_PreservesSemantics()
    {
        var original = "a + b - c";
        var fragments = FragmentParser.Parse(original);
        var terms = AdditiveParser.Parse(fragments);
        var reconstructed = AdditiveParser.Reconstruct(terms);
        FragmentParser.Reconstruct(reconstructed).Should().Be("a+b-c");
    }

    [Fact]
    public void Reconstruct_WithSpaceAroundOperator_AddsSpaces()
    {
        var original = ParserOptions.Default;
        try
        {
            ParserOptions.Default = new ParserOptions { SpaceAroundOperator = true };
            var terms = new List<SignedTerm>
            {
                new(Sign.Positive, FragmentParser.Parse("a")),
                new(Sign.Negative, FragmentParser.Parse("b")),
                new(Sign.Positive, FragmentParser.Parse("c"))
            };
            var result = AdditiveParser.Reconstruct(terms);
            FragmentParser.Reconstruct(result).Should().Be("a - b + c");
        }
        finally
        {
            ParserOptions.Default = original;
        }
    }

    private static string ToString(Fragment[] fragments)
        => new string(fragments.Select(f => f.Character).ToArray());
}
