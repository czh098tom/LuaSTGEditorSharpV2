using System.Reflection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Tests;

public class CodeGenerationContextTests
{
    private static CodeGenerationContext CreateContext(string indentionString = "\t")
    {
        var settings = new CodeGenerationServiceSettings { IndentionString = indentionString };
        return new CodeGenerationContext(
            new ServiceCollection().BuildServiceProvider(),
            new LocalServiceParam(null!),
            settings);
    }

    private static void SetIndentionLevel(CodeGenerationContext context, int value)
    {
        typeof(CodeGenerationContext)
            .GetProperty(nameof(CodeGenerationContext.IndentionLevel))!
            .SetValue(context, value);
    }

    [Fact]
    public void Constructor_DefaultIndentionLevel_Zero()
    {
        var ctx = CreateContext();

        Assert.Equal(0, ctx.IndentionLevel);
    }

    [Fact]
    public void GetIndented_ZeroLevel_ReturnsEmpty()
    {
        var ctx = CreateContext(indentionString: "  ");

        Assert.Equal("", ctx.GetIndented().ToString());
    }

    [Fact]
    public void GetIndented_RepeatsIndentionString()
    {
        var ctx = CreateContext(indentionString: "  ");
        SetIndentionLevel(ctx, 3);

        Assert.Equal("      ", ctx.GetIndented().ToString());
    }

    [Fact]
    public void GetIndented_TabIndention()
    {
        var ctx = CreateContext(indentionString: "\t");
        SetIndentionLevel(ctx, 2);

        Assert.Equal("\t\t", ctx.GetIndented().ToString());
    }
}
