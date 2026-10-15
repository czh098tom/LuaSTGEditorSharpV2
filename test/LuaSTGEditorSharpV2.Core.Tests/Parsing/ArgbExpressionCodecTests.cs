using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing.Argb;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class ArgbExpressionCodecTests
{
    [Fact]
    public void Decode_FourLiteralComponents_ReturnsComponents()
    {
        var result = ArgbExpressionCodec.Decode(
            " 255 , 0x80 , 1e2 , 0x1p7 ");

        result.Should().Be(new ArgbExpression(
            "255",
            "0x80",
            "1e2",
            "0x1p7"));
    }

    [Fact]
    public void CreatePreview_FourLiteralComponents_ReturnsPreview()
    {
        var result = CreatePreview(
            "255",
            "0x80",
            "1e2",
            "0x1p7");

        result.Should().Be(new ArgbPreview(
            255,
            128,
            100,
            128));
    }

    [Fact]
    public void CreatePreview_ExpressionComponents_UseWhiteFallback()
    {
        var result = CreatePreview(
            "alpha",
            "128",
            "blue()",
            "0");

        result.Should().Be(new ArgbPreview(
            255,
            128,
            255,
            0));
    }

    [Fact]
    public void Decode_NestedCommas_DoNotSplitComponents()
    {
        var result = ArgbExpressionCodec.Decode(
            "alpha, clamp(red, 0, 255), " +
            "table[index(1, 2)], { blue, other }[1]");

        result.Should().Be(new ArgbExpression(
            "alpha",
            "clamp(red, 0, 255)",
            "table[index(1, 2)]",
            "{ blue, other }[1]"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1, 2, 3")]
    [InlineData("1, 2, 3, 4, 5")]
    [InlineData("1, , 3, 4")]
    [InlineData(", 2, 3, 4")]
    [InlineData("1, 2, 3,")]
    [InlineData("1, (2, 3, 4")]
    [InlineData("1, 2, @, 4")]
    public void Decode_InvalidStructure_ReturnsNull(
        string input)
    {
        var result = ArgbExpressionCodec.Decode(input);

        result.Should().BeNull();
    }

    [Fact]
    public void CreatePreview_OutOfRangeNumber_ReturnsNull()
    {
        var result = CreatePreview(
            "0",
            "0",
            "256",
            "0");

        result.Should().BeNull();
    }

    [Fact]
    public void CreatePreview_InfiniteLiteral_ReturnsNull()
    {
        var result = CreatePreview(
            "1e9999",
            "0",
            "0",
            "0");

        result.Should().BeNull();
    }

    [Fact]
    public void CreatePreview_InRangeNonIntegerLiteral_FloorsToByte()
    {
        var result = CreatePreview(
            "1.5",
            "0",
            "0",
            "0");

        result.Should().Be(new ArgbPreview(
            1,
            0,
            0,
            0));
    }

    [Fact]
    public void CreatePreview_NegativeNumber_UsesWhiteFallback()
    {
        var result = CreatePreview(
            "-1",
            "0",
            "0",
            "0");

        result.Should().Be(new ArgbPreview(
            255,
            0,
            0,
            0));
    }

    [Fact]
    public void CreatePreview_AllNonNumberComponents_ReturnsNull()
    {
        var result = CreatePreview(
            "alpha",
            "red",
            "green",
            "blue");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("@")]
    [InlineData("(1")]
    public void CreatePreview_InvalidChannel_ReturnsNull(
        string invalidChannel)
    {
        var result = CreatePreview(
            "255",
            invalidChannel,
            "0",
            "0");

        result.Should().BeNull();
    }

    [Fact]
    public void Compose_TrimsComponentsAndUsesStandardSpacing()
    {
        var result = ArgbExpressionCodec.Compose(
            new ArgbExpression(
                " alpha ",
                "128",
                "blue()",
                " 0 "));

        result.Should().Be("alpha, 128, blue(), 0");
    }

    private static ArgbPreview? CreatePreview(
        string alpha,
        string red,
        string green,
        string blue)
    {
        return ArgbExpressionCodec.CreatePreview(
            new ArgbExpression(
                alpha,
                red,
                green,
                blue));
    }
}
