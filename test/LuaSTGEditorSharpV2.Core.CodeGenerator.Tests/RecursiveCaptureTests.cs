using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Tests;

public sealed class RecursiveCaptureTests
{
    [Theory]
    [InlineData(typeof(ConfigurableCodeGeneration))]
    [InlineData(typeof(SelectiveCodeGeneration))]
    [InlineData(typeof(SingleListCodeGeneration))]
    public void NestedGenerationKeepsEachNodesCapturesAndContext(Type generatorType)
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var generator = CreateGenerator(generatorType, provider, services);
        var configuration = CreateConfiguration(generatorType);
        configuration["ContextCaptures"] = JArray.Parse("""
            [{"TypeUID":"Recursive","Property":[{"Key":"name","DisableMacro":true}]}]
            """);
        configuration["Head"] = CreateSection(generatorType, "H{1}:{2}");
        configuration["Tail"] = CreateSection(generatorType, "T{1}:{2}");
        configuration["ChildHead"] = "CH{1}:{2}";
        configuration["ChildTail"] = "CT{1}:{2}";
        JsonConvert.PopulateObject(configuration.ToString(), generator);
        using var registration = provider.Register("Recursive", CreatePackage(), generator);
        var parent = CreateNode("parent");
        var child = CreateNode("child");
        child.Add(CreateNode("leaf"));
        parent.Add(child);
        var disabled = CreateNode("disabled");
        disabled.IsActive = false;
        parent.Add(disabled);

        Assert.Equal(new[] {
            "Hparent:", "CHparent:", "Hchild:parent", "CHchild:parent",
            "Hleaf:child", "Tleaf:child", "CTchild:parent", "Tchild:parent",
            "CTparent:", "Tparent:"
        }, Render(provider, parent, CreateContext(services)));
        Assert.Equal(2, parent.PhysicalChildren.Count);
        Assert.False(parent.PhysicalChildren[1].IsActive);
    }

    [Theory]
    [InlineData(typeof(ConfigurableCodeGeneration))]
    [InlineData(typeof(SelectiveCodeGeneration))]
    [InlineData(typeof(SingleListCodeGeneration))]
    public void InterleavedEnumeratorsKeepIndependentCaptures(Type generatorType)
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var generator = CreateGenerator(generatorType, provider, services);
        var configuration = CreateConfiguration(generatorType);
        configuration["IgnoreChildren"] = true;
        JsonConvert.PopulateObject(configuration.ToString(), generator);
        using var registration = provider.Register("Recursive", CreatePackage(), generator);
        using var first = provider.GenerateCode(CreateNode("first"), CreateContext(services)).GetEnumerator();
        using var second = provider.GenerateCode(CreateNode("second"), CreateContext(services)).GetEnumerator();

        Assert.True(first.MoveNext());
        Assert.Equal("Hfirst", first.Current.Content.Trim());
        Assert.True(second.MoveNext());
        Assert.Equal("Hsecond", second.Current.Content.Trim());
        Assert.True(first.MoveNext());
        Assert.Equal("Tfirst", first.Current.Content.Trim());
        Assert.True(second.MoveNext());
        Assert.Equal("Tsecond", second.Current.Content.Trim());
        Assert.False(first.MoveNext());
        Assert.False(second.MoveNext());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SelectiveTailUsesTheParentsCondition(bool parentEnabled)
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var generator = new SelectiveCodeGeneration(provider, services);
        JsonConvert.PopulateObject("""
            {
              "Captures":[{"Key":"name","DisableMacro":true},{"Key":"enabled","DisableMacro":true}],
              "Head":[{"Text":"H{1}"}],
              "Tail":[{"Text":"T{1}","ConditionOn":1}],
              "IndentionIncrement":0
            }
            """, generator);
        using var registration = provider.Register("Recursive", CreatePackage(), generator);
        var parent = CreateNode("parent");
        parent.Properties["enabled"] = parentEnabled.ToString();
        var child = CreateNode("child");
        child.Properties["enabled"] = (!parentEnabled).ToString();
        parent.Add(child);

        Assert.Equal(parentEnabled
            ? new[] { "Hparent", "Hchild", "Tparent" }
            : new[] { "Hparent", "Hchild", "Tchild" },
            Render(provider, parent, CreateContext(services)).Where(x => x.Length > 0).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void SingleListTailKeepsTheParentsSubCaptures(int childCount)
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var generator = new SingleListCodeGeneration(provider, services);
        JsonConvert.PopulateObject("""
            {
              "Captures":[{"Key":"name","DisableMacro":true}],
              "CountCapture":{"Key":"count"},
              "SubCaptureRule":[{"Key":"item_{0}"}],
              "SubCaptureFormat":[{"Text":"{2}"}],
              "Head":"H{1}:{2}","Tail":"T{1}:{2}",
              "HeadWhenSubCapturesEmpty":"HE{1}","TailWhenSubCapturesEmpty":"TE{1}",
              "IndentionIncrement":0
            }
            """, generator);
        using var registration = provider.Register("Recursive", CreatePackage(), generator);
        var parent = CreateNode("parent");
        parent.Properties["count"] = "2";
        parent.Properties["item_0"] = "outer-a";
        parent.Properties["item_1"] = "outer-b";
        var child = CreateNode("child");
        child.Properties["count"] = childCount.ToString();
        child.Properties["item_0"] = "inner";
        parent.Add(child);

        Assert.Equal(new[] {
            "Hparent:outer-a\nouter-b",
            childCount == 0 ? "HEchild" : "Hchild:inner",
            childCount == 0 ? "TEchild" : "Tchild:inner",
            "Tparent:outer-a\nouter-b"
        }, Render(provider, parent, CreateContext(services)));
    }

    private static CodeGeneratorServiceBase CreateGenerator(Type type,
        CodeGeneratorServiceProvider provider, IServiceProvider services) =>
        (CodeGeneratorServiceBase)Activator.CreateInstance(type, provider, services)!;

    private static JToken CreateSection(Type type, string text) =>
        type == typeof(SelectiveCodeGeneration)
            ? new JArray(new JObject { ["Text"] = text })
            : new JValue(text);

    private static JObject CreateConfiguration(Type type) => new()
    {
        ["Captures"] = JArray.Parse("""[{"Key":"name","DisableMacro":true}]"""),
        ["Head"] = CreateSection(type, "H{1}"),
        ["Tail"] = CreateSection(type, "T{1}"),
        ["IndentionIncrement"] = 0
    };

    private static NodeData CreateNode(string name)
    {
        var node = new NodeData("Recursive");
        node.Properties["name"] = name;
        return node;
    }

    private static string[] Render(CodeGeneratorServiceProvider provider,
        NodeData node, CodeGenerationContext context) =>
        provider.GenerateCode(node, context).Select(x => x.Content.Trim()).ToArray();

    private static CodeGenerationContext CreateContext(IServiceProvider services) =>
        new(services, new LocalServiceParam(null!), new CodeGenerationServiceSettings());

    private static ServiceProvider CreateServices() => new ServiceCollection()
        .AddSingleton<CodeGeneratorServiceProvider>()
        .AddSingleton<LanguageProviderService>()
        .AddSingleton<DefaultValueServiceProvider>()
        .BuildServiceProvider();

    private static PackageInfo CreatePackage() =>
        new(new PackageManifest("test", new Version(1, 0), 0f, null), string.Empty);
}
