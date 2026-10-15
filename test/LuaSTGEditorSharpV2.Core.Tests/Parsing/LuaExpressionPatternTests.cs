using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing;
using LuaSTGEditorSharpV2.Core.Parsing.Legacy;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using LuaSTGEditorSharpV2.Core.Parsing.Lua.Patterns;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class LuaExpressionPatternTests
{
    [Fact]
    public void ComponentPattern_ArbitraryLengthAndNestedGroups_SplitsTopLevelOnly()
    {
        var components = MatchComponents(
            "a,f(b,c),table[x,y]");

        components.Select(Text).Should().Equal(
            "a",
            "f(b,c)",
            "table[x,y]");
    }

    [Fact]
    public void ComponentPattern_WholeParentheses_RecursivelyUnwraps()
    {
        MatchComponents("((a,b))")
            .Select(Text)
            .Should()
            .Equal("a", "b");

        MatchComponents("(a,(b,c))")
            .Select(Text)
            .Should()
            .Equal("a", "(b,c)");
    }

    [Fact]
    public void ComponentPattern_IndividualParentheses_RemainIntact()
    {
        MatchComponents("(a),(b)")
            .Select(Text)
            .Should()
            .Equal("(a)", "(b)");
    }

    [Fact]
    public void ComponentPattern_EmptyComponents_AreSkipped()
    {
        MatchComponents(" a,, ,b, ")
            .Select(Text)
            .Should()
            .Equal("a", "b");
    }

    [Fact]
    public void ComponentPattern_Reconstruct_UsesCommaSpacingOption()
    {
        var pattern = new ComponentPattern(
            new ParserOptions { SpaceAfterComma = true });
        var components = new[]
        {
            Tokenize("a"),
            Tokenize("b"),
            Tokenize("c"),
        };

        Text(pattern.Reconstruct(components))
            .Should()
            .Be("a, b, c");
    }

    [Fact]
    public void AdditiveTermsPattern_NestedOperators_AreNotSplit()
    {
        var pattern = new AdditiveTermsPattern(ParserOptions.Default);
        var terms = pattern.Match(
            Tokenize(" - a + (b-c) - d "));

        terms.Should().NotBeNull();
        terms!.Select(term => term.Sign).Should().Equal(
            Sign.Negative,
            Sign.Positive,
            Sign.Negative);
        terms.Select(term => Text(term.Term)).Should().Equal(
            "a",
            "(b-c)",
            "d");
    }

    [Fact]
    public void AdditiveTermsPattern_Reconstruct_UsesOperatorSpacingOption()
    {
        var pattern = new AdditiveTermsPattern(
            new ParserOptions { SpaceAroundOperator = true });
        var terms = new List<LuaSignedTerm>
        {
            new(Sign.Positive, Tokenize("a")),
            new(Sign.Negative, Tokenize("b")),
            new(Sign.Positive, Tokenize("c")),
        };

        Text(pattern.Reconstruct(terms))
            .Should()
            .Be("a - b + c");
    }

    [Fact]
    public void AdditiveTermsPattern_ConsecutiveSigns_UsesLastSign()
    {
        var pattern = new AdditiveTermsPattern(ParserOptions.Default);
        var terms = pattern.Match(Tokenize("a + - b -- c"));

        terms.Should().NotBeNull();
        terms!.Select(term => term.Sign).Should().Equal(
            Sign.Positive,
            Sign.Negative,
            Sign.Negative);
        terms.Select(term => Text(term.Term)).Should().Equal(
            "a",
            "b",
            "c");
    }

    [Fact]
    public void MultiplicativeFactorsPattern_EmptyFactors_ArePreserved()
    {
        MatchFactors("*a").Select(Text).Should().Equal("", "a");
        MatchFactors("a*").Select(Text).Should().Equal("a", "");
        MatchFactors("a**b").Select(Text).Should().Equal("a", "", "b");
    }

    [Fact]
    public void MultiplicativeFactorsPattern_NestedProducts_AreNotSplit()
    {
        MatchFactors("(a*b)*cos(c*d)")
            .Select(Text)
            .Should()
            .Equal("(a*b)", "cos(c*d)");
    }

    [Fact]
    public void MultiplicativeFactorsPattern_Reconstruct_UsesSpacingOption()
    {
        var pattern = new MultiplicativeFactorsPattern(
            new ParserOptions { SpaceAroundOperator = true });
        var factors = new[]
        {
            Tokenize("radius"),
            Tokenize("cos(angle)"),
        };

        Text(pattern.Reconstruct(factors))
            .Should()
            .Be("radius * cos(angle)");
    }

    private static LuaExpressionToken[][] MatchComponents(string input)
    {
        var pattern = new ComponentPattern(ParserOptions.Default);
        var result = pattern.Match(Tokenize(input));

        result.Should().NotBeNull();
        return result!;
    }

    private static LuaExpressionToken[][] MatchFactors(string input)
    {
        var pattern =
            new MultiplicativeFactorsPattern(ParserOptions.Default);
        var result = pattern.Match(Tokenize(input));

        result.Should().NotBeNull();
        return result!;
    }

    private static LuaExpressionToken[] Tokenize(string input)
    {
        var result = LuaExpressionLexer.Tokenize(input);

        result.Success.Should().BeTrue();
        return result.Value.ToArray();
    }

    private static string Text(IEnumerable<LuaExpressionToken> tokens)
        => string.Concat(tokens.Select(token => token.Text));
}
