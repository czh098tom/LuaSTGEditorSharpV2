using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing.Argb;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class ArgbExpressionCodecTests
{
    [Fact]
    public void Decode_FourLiteralComponents_ProducesPreview()
    {
        var result = ArgbExpressionCodec.Decode(
            " 255 , 0x80 , 1e2 , 0x1p7 ");

        result.Components.Should().Be(new ArgbExpression(
            "255",
            "0x80",
            "1e2",
            "0x1p7"));
        result.Preview.Should().Be(new ArgbPreview(
            new ArgbPreviewChannel(255, false),
            new ArgbPreviewChannel(128, false),
            new ArgbPreviewChannel(100, false),
            new ArgbPreviewChannel(128, false)));
        result.Preview!.Value.UsesExpressionFallback.Should().BeFalse();
    }

    [Fact]
    public void Decode_ExpressionComponents_UsePerChannelFallback()
    {
        var result = ArgbExpressionCodec.Decode(
            "alpha, 128, blue(), 0");

        result.Components.Should().Be(new ArgbExpression(
            "alpha",
            "128",
            "blue()",
            "0"));
        result.Preview.Should().Be(new ArgbPreview(
            new ArgbPreviewChannel(255, true),
            new ArgbPreviewChannel(128, false),
            new ArgbPreviewChannel(255, true),
            new ArgbPreviewChannel(0, false)));
        result.Preview!.Value.UsesExpressionFallback.Should().BeTrue();
    }

    [Fact]
    public void Decode_NestedCommas_DoNotSplitComponents()
    {
        var result = ArgbExpressionCodec.Decode(
            "alpha, clamp(red, 0, 255), " +
            "table[index(1, 2)], { blue, other }[1]");

        result.Components.Should().Be(new ArgbExpression(
            "alpha",
            "clamp(red, 0, 255)",
            "table[index(1, 2)]",
            "{ blue, other }[1]"));
        result.Preview.Should().BeNull();
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
    public void Decode_InvalidStructure_ReturnsNoComponentsOrPreview(
        string input)
    {
        var result = ArgbExpressionCodec.Decode(input);

        result.Components.Should().BeNull();
        result.Preview.Should().BeNull();
    }

    [Fact]
    public void Decode_OutOfRangeNumber_ReturnsNoPreview()
    {
        var result = ArgbExpressionCodec.Decode(
            "0, 0, 256, 0");

        result.Components.Should().Be(new ArgbExpression(
            "0",
            "0",
            "256",
            "0"));
        result.Preview.Should().BeNull();
    }

    [Fact]
    public void Decode_InfiniteLiteral_ReturnsNoPreview()
    {
        var result = ArgbExpressionCodec.Decode(
            "1e9999, 0, 0, 0");

        result.Components.Should().Be(new ArgbExpression(
            "1e9999",
            "0",
            "0",
            "0"));
        result.Preview.Should().BeNull();
    }

    [Fact]
    public void Decode_InRangeNonIntegerLiteral_FloorToByte()
    {
        var result = ArgbExpressionCodec.Decode(
            "1.5, 0, 0, 0");

        result.Preview.Should().Be(new ArgbPreview(
            new ArgbPreviewChannel(1, false),
            new ArgbPreviewChannel(0, false),
            new ArgbPreviewChannel(0, false),
            new ArgbPreviewChannel(0, false)));
        result.Preview!.Value.UsesExpressionFallback.Should().BeFalse();
    }

    [Fact]
    public void Decode_NegativeNumber_IsNonNumberExpression()
    {
        var result = ArgbExpressionCodec.Decode(
            "-1, 0, 0, 0");

        result.Components.Should().Be(new ArgbExpression(
            "-1",
            "0",
            "0",
            "0"));
        result.Preview.Should().Be(new ArgbPreview(
            new ArgbPreviewChannel(255, true),
            new ArgbPreviewChannel(0, false),
            new ArgbPreviewChannel(0, false),
            new ArgbPreviewChannel(0, false)));
    }

    [Fact]
    public void Decode_AllNonNumberComponents_ReturnsNoPreview()
    {
        var result = ArgbExpressionCodec.Decode(
            "alpha, red, green, blue");

        result.Components.Should().Be(new ArgbExpression(
            "alpha",
            "red",
            "green",
            "blue"));
        result.Preview.Should().BeNull();
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
}
