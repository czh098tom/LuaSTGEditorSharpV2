using FluentAssertions;
using LuaSTGEditorSharpV2.Core.Parsing;
using LuaSTGEditorSharpV2.Core.Parsing.Vector2;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Parsing;

[Collection("Parser Tests")]
public class Vector2ExpressionCodecTests
{
    [Theory]
    [InlineData("", "", "")]
    [InlineData("x", "x", "")]
    [InlineData("x,y", "x", "y")]
    [InlineData(" x , y , z ", "x", "y")]
    [InlineData("a,,b", "a", "b")]
    [InlineData("f(a,b),table[i,j]", "f(a,b)", "table[i,j]")]
    [InlineData("((a,b))", "a", "b")]
    public void Decode_SplitsTopLevelComponents(
        string input,
        string expectedX,
        string expectedY)
    {
        Vector2ExpressionCodec.Decode(input)
            .Cartesian
            .Should()
            .Be(new Vector2ExpressionPair(expectedX, expectedY));
    }

    [Fact]
    public void Decode_LexerFailure_UsesWholeInputAsCartesianX()
    {
        var result = Vector2ExpressionCodec.Decode("a @ b");

        result.Cartesian.Should().Be(
            new Vector2ExpressionPair("a @ b", string.Empty));
        result.Components.Should().ContainSingle();
        result.Components[0].Polar.Should().BeNull();
        result.Components[0].MemberAccess.Should().BeNull();
        result.Components[0].IndexAccess.Should().BeNull();
    }

    [Theory]
    [InlineData("", "y")]
    [InlineData(" ", "y")]
    public void DecodeComponent_PreservesSeparatedCartesianAxes(
        string x,
        string y)
    {
        var expression = new Vector2ExpressionPair(x, y);

        var result = Vector2ExpressionCodec.DecodeComponent(expression);

        result.Cartesian.Should().Be(expression);
    }

    [Fact]
    public void Decode_RecognizesEachSubVectorCandidate()
    {
        var result = Vector2ExpressionCodec.Decode(
            "r*cos(a)+factory()[key].x,"
            + "r*sin(a)+factory()[key].y");

        result.Components.Should().HaveCount(2);
        result.Components[0].Polar.Should().Be(
            new PolarVector2Form("r", "a"));
        result.Components[1].MemberAccess.Should().Be(
            new MemberAccessVector2Form(
                "factory()[key]",
                "x",
                "y"));
    }

    [Fact]
    public void Decode_ReferenceSourceComparison_IgnoresWhitespace()
    {
        var result = Vector2ExpressionCodec.Decode(
            "factory() [key].x,factory()[key].y");

        result.Components.Should().ContainSingle();
        result.Components[0].MemberAccess.Should().Be(
            new MemberAccessVector2Form(
                "factory() [key]",
                "x",
                "y"));
    }

    [Theory]
    [InlineData("foo..x,foo..y")]
    [InlineData("left or right.x,left or right.y")]
    public void Decode_InvalidPrefixSource_HasNoReferenceCandidate(
        string input)
    {
        var result = Vector2ExpressionCodec.Decode(input);

        result.Components.Should().ContainSingle();
        result.Components[0].MemberAccess.Should().BeNull();
        result.Components[0].IndexAccess.Should().BeNull();
    }

    [Theory]
    [InlineData("(left or right).x,(left or right).y", "(left or right)")]
    [InlineData("factory()[key].x,factory()[key].y", "factory()[key]")]
    public void Decode_ValidPrefixSource_HasMemberCandidate(
        string input,
        string expectedSource)
    {
        var result = Vector2ExpressionCodec.Decode(input);

        result.Components.Should().ContainSingle();
        result.Components[0].MemberAccess!.Source
            .Should()
            .Be(expectedSource);
    }

    [Fact]
    public void Decode_IndexAccess_ExtractsBothAccessors()
    {
        var result = Vector2ExpressionCodec.Decode(
            "vectors[i][left+1],vectors[i][right+1]");

        result.Components.Should().ContainSingle();
        result.Components[0].IndexAccess.Should().Be(
            new IndexAccessVector2Form(
                "vectors[i]",
                "left+1",
                "right+1"));
    }

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
    [InlineData("(speed+offset)*cos(angle+delta)", "(speed+offset)*sin(angle+delta)", "(speed+offset)", "angle+delta")]
    public void DecodeComponent_ValidPolarPair_ExtractsForm(
        string x,
        string y,
        string expectedRadius,
        string expectedAngle)
    {
        var result = Vector2ExpressionCodec.DecodeComponent(
            new Vector2ExpressionPair(x, y));

        result.Polar.Should().Be(
            new PolarVector2Form(expectedRadius, expectedAngle));
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
    public void DecodeComponent_NonPolarPair_HasNoPolarCandidate(
        string x,
        string y)
    {
        var result = Vector2ExpressionCodec.DecodeComponent(
            new Vector2ExpressionPair(x, y));

        result.Polar.Should().BeNull();
    }

    [Theory]
    [InlineData("vector.x", "vector.y", "vector", "x", "y")]
    [InlineData("owner.velocity.x", "owner.velocity.y", "owner.velocity", "x", "y")]
    public void DecodeComponent_MatchingMemberReferences_ExtractsForm(
        string x,
        string y,
        string expectedSource,
        string expectedFirstAccessor,
        string expectedSecondAccessor)
    {
        var result = Vector2ExpressionCodec.DecodeComponent(
            new Vector2ExpressionPair(x, y));

        result.MemberAccess.Should().Be(
            new MemberAccessVector2Form(
                expectedSource,
                expectedFirstAccessor,
                expectedSecondAccessor));
    }

    [Theory]
    [InlineData("vectors[i][a]", "vectors[i][b]", "vectors[i]", "a", "b")]
    [InlineData("get_vector()[left+1]", "get_vector()[right+1]", "get_vector()", "left+1", "right+1")]
    [InlineData("table[\"position\"][1]", "table[\"position\"][2]", "table[\"position\"]", "1", "2")]
    public void DecodeComponent_MatchingIndexReferences_ExtractsForm(
        string x,
        string y,
        string expectedSource,
        string expectedFirstAccessor,
        string expectedSecondAccessor)
    {
        var result = Vector2ExpressionCodec.DecodeComponent(
            new Vector2ExpressionPair(x, y));

        result.IndexAccess.Should().Be(
            new IndexAccessVector2Form(
                expectedSource,
                expectedFirstAccessor,
                expectedSecondAccessor));
    }

    [Theory]
    [InlineData("left.x", "right.y")]
    [InlineData("source[a]", "other[b]")]
    [InlineData("source.x", "source[y]")]
    [InlineData("source.1", "source.2")]
    [InlineData("source[a", "source[b]")]
    [InlineData("left+right[a]", "left+right[b]")]
    [InlineData("source[]", "source[]")]
    [InlineData("source[ ]", "source[\t]")]
    [InlineData("source[][a]", "source[][b]")]
    public void DecodeComponent_NonMatchingReferences_HaveNoCandidate(
        string x,
        string y)
    {
        var result = Vector2ExpressionCodec.DecodeComponent(
            new Vector2ExpressionPair(x, y));

        result.MemberAccess.Should().BeNull();
        result.IndexAccess.Should().BeNull();
    }
    [Theory]
    [InlineData("r", "a", "r*cos(a)", "r*sin(a)")]
    [InlineData("1", "a", "cos(a)", "sin(a)")]
    [InlineData("-1", "a", "-cos(a)", "-sin(a)")]
    [InlineData("a+b", "t", "(a+b)*cos(t)", "(a+b)*sin(t)")]
    public void PolarCompose_FormatsEditableDraft(
        string radius,
        string angle,
        string expectedX,
        string expectedY)
    {
        Vector2ExpressionCodec.Polar.Compose(radius, angle)
            .Should()
            .Be(new Vector2ExpressionPair(expectedX, expectedY));
    }

    [Fact]
    public void ReferenceCompose_UsesSelectedSyntax()
    {
        Vector2ExpressionCodec.MemberAccess.Compose(
                "vector",
                "x",
                "y")
            .Should()
            .Be(new Vector2ExpressionPair(
                "vector.x",
                "vector.y"));
        Vector2ExpressionCodec.IndexAccess.Compose(
                "vector",
                "1",
                "2")
            .Should()
            .Be(new Vector2ExpressionPair(
                "vector[1]",
                "vector[2]"));
    }

    [Fact]
    public void ComposeComponents_MergesTermsAndDropsTrailingZeroes()
    {
        Vector2ExpressionCodec.ComposeComponents(
                new[]
                {
                    new Vector2ExpressionPair("a", "b"),
                    new Vector2ExpressionPair("-c", "d"),
                    new Vector2ExpressionPair("0", "0"),
                })
            .Should()
            .Be(new Vector2ExpressionPair("a-c", "b+d"));
    }
}

