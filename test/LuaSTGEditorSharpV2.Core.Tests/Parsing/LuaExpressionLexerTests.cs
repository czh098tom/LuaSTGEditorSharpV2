using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class LuaExpressionLexerTests
{
    [Fact]
    public void Tokenize_EmptyInput_ReturnsEmptyTokenList()
    {
        var result = LuaExpressionLexer.Tokenize(string.Empty);

        result.Success.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Theory]
    [InlineData("alpha", "alpha")]
    [InlineData("_beta", "_beta")]
    [InlineData("gamma2", "gamma2")]
    public void Tokenize_Identifier_ProducesIdentifierAtom(string input, string expectedText)
    {
        var tokens = Tokenize(input);

        tokens.Should().ContainSingle()
            .Which.Should().BeOfType<LuaExpressionAtom>()
            .Which.Kind.Should().Be(LuaExpressionAtomKind.Identifier);
        tokens[0].Text.Should().Be(expectedText);
    }

    [Theory]
    [InlineData("and")]
    [InlineData("or")]
    [InlineData("not")]
    [InlineData("local")]
    [InlineData("nil")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("goto")]
    public void Tokenize_Keyword_ProducesKeywordAtom(string input)
    {
        Tokenize(input).Should().ContainSingle()
            .Which.Should().BeOfType<LuaExpressionAtom>()
            .Which.Kind.Should().Be(LuaExpressionAtomKind.Keyword);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("42")]
    [InlineData("3.14")]
    [InlineData("1e10")]
    [InlineData("2.5e-3")]
    [InlineData("0xff")]
    [InlineData("0X10")]
    [InlineData("0x1.8p2")]
    [InlineData("0x1p-2")]
    public void Tokenize_Number_ProducesSingleNumberAtom(string input)
    {
        var tokens = Tokenize(input);

        tokens.Should().ContainSingle()
            .Which.Should().BeOfType<LuaExpressionAtom>()
            .Which.Kind.Should().Be(LuaExpressionAtomKind.Number);
        tokens[0].Text.Should().Be(input);
    }

    [Fact]
    public void Tokenize_TwoMinusSigns_AreTwoOperatorsNotComment()
    {
        Significant(Tokenize("a--b")).Should().Equal(
            (LuaExpressionAtomKind.Identifier, "a"),
            (LuaExpressionAtomKind.Operator, "-"),
            (LuaExpressionAtomKind.Operator, "-"),
            (LuaExpressionAtomKind.Identifier, "b"));
    }

    [Theory]
    [InlineData("a..b", "..", LuaExpressionAtomKind.Operator)]
    [InlineData("a...b", "...", LuaExpressionAtomKind.Punctuation)]
    [InlineData("a==b", "==", LuaExpressionAtomKind.Operator)]
    [InlineData("a~=b", "~=", LuaExpressionAtomKind.Operator)]
    [InlineData("a<=b", "<=", LuaExpressionAtomKind.Operator)]
    [InlineData("a>=b", ">=", LuaExpressionAtomKind.Operator)]
    public void Tokenize_LongestOperator_IsNotSplit(
        string input,
        string expectedText,
        LuaExpressionAtomKind expectedKind)
    {
        Significant(Tokenize(input))[1].Should().Be((expectedKind, expectedText));
    }

    [Theory]
    [InlineData("\"hello\"")]
    [InlineData("'world'")]
    [InlineData("\"a\\\"b\"")]
    [InlineData("'a\\'b'")]
    public void Tokenize_ShortString_ProducesStringAtom(string input)
    {
        var tokens = Tokenize(input);

        tokens.Should().ContainSingle()
            .Which.Should().BeOfType<LuaExpressionAtom>()
            .Which.Kind.Should().Be(LuaExpressionAtomKind.String);
        Reconstruct(tokens).Should().Be(input);
    }

    [Theory]
    [InlineData("[[hello]]")]
    [InlineData("[=[a]b]=]")]
    [InlineData("[==[x]==]")]
    public void Tokenize_LongString_ProducesStringAtom(string input)
    {
        var tokens = Tokenize(input);

        tokens.Should().ContainSingle()
            .Which.Should().BeOfType<LuaExpressionAtom>()
            .Which.Kind.Should().Be(LuaExpressionAtomKind.String);
        Reconstruct(tokens).Should().Be(input);
    }

    [Fact]
    public void Tokenize_NestedGroups_ProducesRecursiveGroupTree()
    {
        var tokens = Tokenize("(a[b]{c})");

        var group = tokens.Should().ContainSingle()
            .Which.Should().BeOfType<LuaExpressionGroup>()
            .Subject;
        group.Kind.Should().Be(LuaExpressionGroupKind.Parentheses);
        group.Children.Should().HaveCount(3);
        group.Children[0].Should().BeOfType<LuaExpressionAtom>()
            .Which.Text.Should().Be("a");
        group.Children[1].Should().BeOfType<LuaExpressionGroup>()
            .Which.Kind.Should().Be(LuaExpressionGroupKind.Brackets);
        group.Children[2].Should().BeOfType<LuaExpressionGroup>()
            .Which.Kind.Should().Be(LuaExpressionGroupKind.Braces);
    }

    [Fact]
    public void ExpressionGroup_Children_AreDefensivelyCopiedAndReadOnly()
    {
        var original = new LuaExpressionAtom(
            "a",
            LuaExpressionAtomKind.Identifier);
        LuaExpressionToken[] children = [original];
        var group = new LuaExpressionGroup(
            LuaExpressionGroupKind.Parentheses,
            "(",
            ")",
            children);

        children[0] = new LuaExpressionAtom(
            "b",
            LuaExpressionAtomKind.Identifier);

        group.Children.Should().ContainSingle()
            .Which.Should().BeSameAs(original);
        var mutation = () =>
            ((IList<LuaExpressionToken>)group.Children)[0] = children[0];
        mutation.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [InlineData("(")]
    [InlineData(")")]
    [InlineData("[")]
    [InlineData("]")]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("(]")]
    [InlineData("([)]")]
    [InlineData("([{})")]
    [InlineData("a)")]
    [InlineData("(a")]
    public void Tokenize_UnbalancedOrMismatchedGroups_Fails(string input)
        => Fails(input);

    [Theory]
    [InlineData("\"abc")]
    [InlineData("'abc")]
    [InlineData("[[abc")]
    [InlineData("[=[abc")]
    public void Tokenize_UnclosedStrings_Fails(string input)
        => Fails(input);

    [Theory]
    [InlineData("@")]
    [InlineData("$")]
    [InlineData("?")]
    [InlineData("3abc")]
    public void Tokenize_IllegalTokens_Fails(string input)
        => Fails(input);

    [Theory]
    [InlineData("a + (b - c) * vector[i]")]
    [InlineData("foo..bar and (left or right).x")]
    [InlineData("\"a\" .. [[b]] .. 'c'")]
    public void Tokenize_RoundTrip_PreservesOriginalText(string input)
    {
        Reconstruct(Tokenize(input)).Should().Be(input);
    }

    private static IReadOnlyList<LuaExpressionToken> Tokenize(string input)
    {
        var result = LuaExpressionLexer.Tokenize(input);

        result.Success.Should().BeTrue();
        return result.Value;
    }

    private static void Fails(string input)
    {
        LuaExpressionLexer.Tokenize(input).Success.Should().BeFalse();
    }

    private static string Reconstruct(IEnumerable<LuaExpressionToken> tokens)
        => string.Concat(tokens.Select(token => token.Text));

    private static (LuaExpressionAtomKind Kind, string Text)[] Significant(
        IReadOnlyList<LuaExpressionToken> tokens)
        => tokens
            .OfType<LuaExpressionAtom>()
            .Where(atom => atom.Kind != LuaExpressionAtomKind.Whitespace)
            .Select(atom => (atom.Kind, atom.Text))
            .ToArray();
}
