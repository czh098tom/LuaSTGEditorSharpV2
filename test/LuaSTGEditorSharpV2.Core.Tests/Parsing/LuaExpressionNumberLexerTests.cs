using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing.Lua;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class LuaExpressionNumberLexerTests
{
    [Theory]
    [InlineData("0", 0L)]
    [InlineData("00", 0L)]
    [InlineData("0123", 123L)]
    [InlineData("123", 123L)]
    [InlineData("0x0", 0L)]
    [InlineData("0xff", 255L)]
    [InlineData("0XFF", 255L)]
    [InlineData("0xDEADBEEF", 3735928559L)]
    [InlineData("0x1000000000000000", 1152921504606846976L)]
    public void Tokenize_Integer_BoxesInt64Value(
        string input,
        long expected)
    {
        var atom = TokenizeNumber(input);

        atom.Text.Should().Be(input);
        atom.Value.Should().BeOfType<long>()
            .Which.Should().Be(expected);
    }

    [Theory]
    [InlineData("123.456", 123.456d)]
    [InlineData(".5", 0.5d)]
    [InlineData("1.", 1d)]
    [InlineData("1.e2", 100d)]
    [InlineData("1e2", 100d)]
    [InlineData("1e+2", 100d)]
    [InlineData("1e-2", 0.01d)]
    [InlineData("123.456e-2", 1.23456d)]
    [InlineData("0x1.", 1d)]
    [InlineData("0x1.f", 1.9375d)]
    [InlineData("0x1.fp3", 15.5d)]
    [InlineData("0x1.8p-2", 0.375d)]
    [InlineData("0x1.p3", 8d)]
    [InlineData("0x1p3", 8d)]
    [InlineData("0x1P+3", 8d)]
    [InlineData("0X1P-1", 0.5d)]
    [InlineData("0x1000000000000.8p4", 4503599627370504d)]
    public void Tokenize_Float_BoxesDoubleValue(
        string input,
        double expected)
    {
        var atom = TokenizeNumber(input);

        atom.Text.Should().Be(input);
        atom.Value.Should().BeOfType<double>()
            .Which.Should().BeApproximately(
                expected,
                1e-12 * Math.Max(1d, Math.Abs(expected)));
    }

    [Theory]
    [InlineData("9223372036854775808", 9223372036854775808d)]
    [InlineData("0x8000000000000000", 9223372036854775808d)]
    public void Tokenize_IntegerOutsideInt64_BoxesDoubleValue(
        string input,
        double expected)
    {
        var atom = TokenizeNumber(input);

        atom.Value.Should().BeOfType<double>()
            .Which.Should().Be(expected);
    }

    [Fact]
    public void Tokenize_OutOfRangeDecimal_BoxesInfinityWithoutFailing()
    {
        var atom = TokenizeNumber("1e9999");

        atom.Value.Should().BeOfType<double>()
            .Which.Should().Be(double.PositiveInfinity);
    }

    [Theory]
    [InlineData("1e")]
    [InlineData("1e+")]
    [InlineData("1e-")]
    [InlineData("0x")]
    [InlineData("0x1p")]
    [InlineData("0x1p+")]
    [InlineData("0x1p-")]
    [InlineData("0x.")]
    [InlineData("0x1g")]
    [InlineData("123abc")]
    [InlineData("1..2")]
    public void Tokenize_MalformedNumber_Fails(string input)
        => LuaExpressionLexer.Tokenize(input).Success.Should().BeFalse();

    private static LuaExpressionAtom TokenizeNumber(string input)
    {
        var result = LuaExpressionLexer.Tokenize(input);

        result.Success.Should().BeTrue();
        var atom = result.Value.Should().ContainSingle()
            .Which.Should().BeOfType<LuaExpressionAtom>()
            .Which;
        atom.Kind.Should().Be(LuaExpressionAtomKind.Number);
        atom.Value.Should().NotBeNull();
        return atom;
    }
}
