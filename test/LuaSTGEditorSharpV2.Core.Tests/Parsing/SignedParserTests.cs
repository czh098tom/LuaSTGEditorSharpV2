using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class SignedParserTests
{
    [Fact]
    public void Parse_Empty_ReturnsEmptyAtom()
    {
        var fragments = FragmentParser.Parse("");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Positive);
        result.Term.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NoSign_ReturnsPositive()
    {
        var fragments = FragmentParser.Parse("abc");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Positive);
        ToString(result.Term).Should().Be("abc");
    }

    [Fact]
    public void Parse_WithPlus_ReturnsPositive()
    {
        var fragments = FragmentParser.Parse("+abc");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Positive);
        ToString(result.Term).Should().Be("abc");
    }

    [Fact]
    public void Parse_WithMinus_ReturnsNegative()
    {
        var fragments = FragmentParser.Parse("-abc");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Negative);
        ToString(result.Term).Should().Be("abc");
    }

    [Fact]
    public void Parse_TrimsWhitespace()
    {
        var fragments = FragmentParser.Parse("  -  abc  ");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Negative);
        ToString(result.Term).Should().Be("abc");
    }

    [Fact]
    public void Parse_MinusInsideParentheses_NotTreatedAsSign()
    {
        var fragments = FragmentParser.Parse("(-a)");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Positive);
        ToString(result.Term).Should().Be("(-a)");
    }

    [Fact]
    public void Parse_ComplexExpression_ParsesCorrectly()
    {
        var fragments = FragmentParser.Parse("-a(b + c)");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Negative);
        ToString(result.Term).Should().Be("a(b + c)");
    }

    [Fact]
    public void Parse_PlusInsideParentheses_NotTreatedAsSign()
    {
        var fragments = FragmentParser.Parse("(+a)");
        var result = SignedParser.Parse(fragments);
        result.Sign.Should().Be(Sign.Positive);
        ToString(result.Term).Should().Be("(+a)");
    }

    [Fact]
    public void Reconstruct_Positive_NoMinus()
    {
        var term = new SignedTerm(Sign.Positive, FragmentParser.Parse("abc"));
        var result = SignedParser.Reconstruct(term);
        FragmentParser.Reconstruct(result).Should().Be("abc");
    }

    [Fact]
    public void Reconstruct_Negative_PrependsMinus()
    {
        var term = new SignedTerm(Sign.Negative, FragmentParser.Parse("abc"));
        var result = SignedParser.Reconstruct(term);
        FragmentParser.Reconstruct(result).Should().Be("-abc");
    }

    [Fact]
    public void Reconstruct_EmptyAtom_ReturnsEmpty()
    {
        var term = new SignedTerm(Sign.Negative, []);
        var result = SignedParser.Reconstruct(term);
        result.Should().BeEmpty();
    }

    [Fact]
    public void RoundTrip_Positive_PreservesValue()
    {
        var original = "abc";
        var fragments = FragmentParser.Parse(original);
        var parsed = SignedParser.Parse(fragments);
        var reconstructed = SignedParser.Reconstruct(parsed);
        FragmentParser.Reconstruct(reconstructed).Should().Be(original);
    }

    [Fact]
    public void RoundTrip_Negative_PreservesValue()
    {
        var original = "-abc";
        var fragments = FragmentParser.Parse(original);
        var parsed = SignedParser.Parse(fragments);
        var reconstructed = SignedParser.Reconstruct(parsed);
        FragmentParser.Reconstruct(reconstructed).Should().Be(original);
    }

    [Fact]
    public void RoundTrip_ComplexAtom_PreservesValue()
    {
        var original = "-(a + b)";
        var fragments = FragmentParser.Parse(original);
        var parsed = SignedParser.Parse(fragments);
        var reconstructed = SignedParser.Reconstruct(parsed);
        FragmentParser.Reconstruct(reconstructed).Should().Be(original);
    }

    [Fact]
    public void Reconstruct_WithSpaceAfterSign_AddsSpace()
    {
        var original = ParserOptions.Default;
        try
        {
            ParserOptions.Default = new ParserOptions { SpaceAroundOperator = true };
            var term = new SignedTerm(Sign.Negative, FragmentParser.Parse("abc"));
            var result = SignedParser.Reconstruct(term);
            FragmentParser.Reconstruct(result).Should().Be("- abc");
        }
        finally
        {
            ParserOptions.Default = original;
        }
    }

    private static string ToString(Fragment[] fragments)
        => new string(fragments.Select(f => f.Character).ToArray());
}
