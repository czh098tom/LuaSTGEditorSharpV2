using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing.Argb;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class ArgbExpressionCodecTests
{
    [Fact]
    public void Decode_FourLiteralComponents_ProducesStructuredPreview()
    {
        const string input = " 255 , 0x80 , 1e2 , 0x1p7 ";

        var result = ArgbExpressionCodec.Decode(input);

        result.OriginalText.Should().Be(input);
        result.IsStructured.Should().BeTrue();
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

        result.IsStructured.Should().BeTrue();
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

        result.IsStructured.Should().BeTrue();
        result.Components.Should().Be(new ArgbExpression(
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
    public void Decode_InvalidStructure_PreservesOriginalText(
        string input)
    {
        var result = ArgbExpressionCodec.Decode(input);

        result.OriginalText.Should().Be(input);
        result.IsStructured.Should().BeFalse();
        result.Components.Should().BeNull();
        result.Preview.Should().BeNull();
    }

    [Fact]
    public void Decode_NonByteLiteralsAndGroupedValue_UseFallback()
    {
        var result = ArgbExpressionCodec.Decode(
            "-1, 1.5, 256, (0)");

        result.Preview.Should().Be(new ArgbPreview(
            new ArgbPreviewChannel(255, true),
            new ArgbPreviewChannel(255, true),
            new ArgbPreviewChannel(255, true),
            new ArgbPreviewChannel(255, true)));
    }

    [Fact]
    public void Decode_InfiniteLiteral_UsesFallback()
    {
        var result = ArgbExpressionCodec.Decode(
            "1e9999, 0, 0, 0");

        result.Preview!.Value.Alpha.Should().Be(
            new ArgbPreviewChannel(255, true));
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
