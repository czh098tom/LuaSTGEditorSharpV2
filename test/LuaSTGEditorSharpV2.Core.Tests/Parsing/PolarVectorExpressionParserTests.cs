using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing.Facade;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class PolarVectorExpressionParserTests
{
    [Theory]
    [InlineData("r*cos(a)", "r*sin(a)", "r", "a")]
    [InlineData("cos(a)*r", "sin(a)*r", "r", "a")]
    [InlineData("m*n*cos(a)", "m*n*sin(a)", "m*n", "a")]
    [InlineData("(r*cos(a))", "(r*sin(a))", "r", "a")]
    [InlineData("((r*cos(a)))", "((r*sin(a)))", "r", "a")]
    [InlineData("cos(a)", "sin(a)", "1", "a")]
    [InlineData("r*cos (a)", "r*sin (a)", "r", "a")]
    [InlineData("r*cos(f(a, g(b)))", "r*sin(f(a, g(b)))", "r", "f(a, g(b))")]
    [InlineData("-r*cos(a)", "-r*sin(a)", "-r", "a")]
    public void TryDecompose_ValidPolarPair_ReturnsRadiusAndAngle(
        string x,
        string y,
        string expectedRadius,
        string expectedAngle)
    {
        var parsed = PolarVectorExpressionParser.TryDecompose(
            x,
            y,
            out var radius,
            out var angle);

        parsed.Should().BeTrue();
        radius.Should().Be(expectedRadius);
        angle.Should().Be(expectedAngle);
    }

    [Fact]
    public void TryDecompose_NestedRadiusAndAngle_PreservesExpressions()
    {
        var parsed = PolarVectorExpressionParser.TryDecompose(
            "(speed+offset)*cos(angle+delta)",
            "(speed+offset)*sin(angle+delta)",
            out var radius,
            out var angle);

        parsed.Should().BeTrue();
        radius.Should().Be("(speed+offset)");
        angle.Should().Be("angle+delta");
    }

    [Theory]
    [InlineData("r*cos(a)", "q*sin(a)")]
    [InlineData("r*cos(a)", "r*sin(b)")]
    [InlineData("r*sin(a)", "r*cos(a)")]
    [InlineData("r*cos(a)", "-r*sin(a)")]
    [InlineData("a*b*cos(t)", "b*a*sin(t)")]
    [InlineData("r**cos(a)", "r*sin(a)")]
    [InlineData("a-b*cos(t)", "a-b*sin(t)")]
    [InlineData("r*cos(a))", "r*sin(a)")]
    [InlineData("r*cos(a", "r*sin(a)")]
    [InlineData("r*cos()", "r*sin()")]
    [InlineData("r*cos(a)+tail", "r*sin(a)+tail")]
    public void TryDecompose_NonMatchingPair_ReturnsFalse(string x, string y)
    {
        var parsed = PolarVectorExpressionParser.TryDecompose(
            x,
            y,
            out var radius,
            out var angle);

        parsed.Should().BeFalse();
        radius.Should().BeEmpty();
        angle.Should().BeEmpty();
    }

    [Theory]
    [InlineData("r", "a", "r*cos(a)", "r*sin(a)")]
    [InlineData("1", "a", "cos(a)", "sin(a)")]
    [InlineData("-1", "a", "-cos(a)", "-sin(a)")]
    [InlineData("a+b", "t", "(a+b)*cos(t)", "(a+b)*sin(t)")]
    public void Compose_FormatsCanonicalComponents(
        string radius,
        string angle,
        string expectedX,
        string expectedY)
    {
        var result = PolarVectorExpressionParser.Compose(radius, angle);

        result.X.Should().Be(expectedX);
        result.Y.Should().Be(expectedY);
    }
}
