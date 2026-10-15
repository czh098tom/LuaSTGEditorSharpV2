using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Tests;

public sealed class MustacheHelperTests
{
    [Fact]
    public void BothPathsCountOnlyDirectActiveChildren()
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var context = new CodeGenerationContext(
            services, new LocalServiceParam(null!), new CodeGenerationServiceSettings());
        const string template =
            "{{logicalChildrenCount}}:{{#condition.has_children}}yes{{/condition.has_children}}" +
            "{{^condition.has_children}}no{{/condition.has_children}}";

        foreach (var (active, disabled) in new[] { (0, 0), (0, 2), (1, 0), (1, 2), (3, 1) })
        {
            var node = new NodeData("Helper");
            for (var i = 0; i < active + disabled; i++)
            {
                var child = new NodeData("child") { IsActive = i < active };
                child.Add(new NodeData("grandchild"));
                node.Add(child);
            }
            var conditions = new Dictionary<string, string>
            {
                ["has_children"] = "{{logicalChildrenCount}} > 0"
            };
            var generator = new MustacheCodeGeneration(provider, services);
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(new
            {
                IgnoreChildren = true,
                Head = new[] { template },
                ConditionTemplates = conditions
            }), generator);
            using var registration = provider.Register("Helper", CreatePackage(), generator);
            var expected = $"{active}:{(active > 0 ? "yes" : "no")}";
            Assert.Equal(expected,
                string.Concat(provider.GenerateCode(node, context).Select(x => x.Content)).Trim());

            var model = CodeFragmentTemplate.CreateModel(services, node, context,
                new Dictionary<string, CaptureWithMacroOption>(),
                new Dictionary<string, ContextCapture>(), conditions);
            Assert.Equal(expected, CodeFragmentTemplate.RenderText([template], model, context));
        }
    }

    [Fact]
    public void RecursiveGenerationKeepsParentHelperAndConditionsInTail()
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var context = new CodeGenerationContext(
            services, new LocalServiceParam(null!), new CodeGenerationServiceSettings());
        var parent = new NodeData("Helper");
        var child = new NodeData("Helper");
        child.Add(new NodeData("leaf"));
        parent.Add(child);
        parent.Add(new NodeData("leaf"));

        var generator = new MustacheCodeGeneration(provider, services);
        JsonConvert.PopulateObject("""
            {
              "Head": ["H{{logicalChildrenCount}}{{#condition.two}}T{{/condition.two}}"],
              "Tail": ["T{{logicalChildrenCount}}{{#condition.two}}T{{/condition.two}}"],
              "ConditionTemplates": { "two": "{{logicalChildrenCount}} == 2" }
            }
            """, generator);
        using var registration = provider.Register("Helper", CreatePackage(), generator);
        Assert.Equal(new[] { "H2T", "H1", "T1", "T2T" },
            provider.GenerateCode(parent, context).Select(x => x.Content.Trim()).ToArray());
    }

    [Theory]
    [InlineData("\t")]
    [InlineData("  ")]
    [InlineData("")]
    public void BothPathsExpandOnlyTemplateIndentTokens(string indent)
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var context = new CodeGenerationContext(services, new LocalServiceParam(null!),
            new CodeGenerationServiceSettings { IndentionString = indent });
        var node = new NodeData("Indent");
        node.Properties["value"] = "{0:IND}";
        var generator = new MustacheCodeGeneration(provider, services);
        const string template = "{0:IND}{{{self.value}}}";
        JsonConvert.PopulateObject(JsonConvert.SerializeObject(new
        {
            Captures = new { value = new { Key = "value", DisableMacro = true } },
            Head = new[] { template },
            Tail = new[] { template },
            IgnoreChildren = true
        }), generator);
        using var registration = provider.Register("Indent", CreatePackage(), generator);
        using var outer = context.AcquireContextHandle(new NodeData("outer"), 1);
        Assert.Equal(new[] { indent + indent + "{0:IND}\n", indent + indent + "{0:IND}\n" },
            provider.GenerateCode(node, context).Select(x => x.Content).ToArray());

        var model = CodeFragmentTemplate.CreateModel(services, node, context,
            generator.Captures, new Dictionary<string, ContextCapture>(), new Dictionary<string, string>());
        Assert.Equal(indent + "{0:IND}", CodeFragmentTemplate.RenderText([template], model, context));
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(" \t\r\n", false)]
    [InlineData("\u2000", false)]
    [InlineData("a, b", true)]
    public void ParameterSectionsTreatWhitespaceAsAbsent(string parameters, bool expected)
    {
        using var services = CreateServices();
        var provider = services.GetRequiredService<CodeGeneratorServiceProvider>();
        var context = new CodeGenerationContext(
            services, new LocalServiceParam(null!), new CodeGenerationServiceSettings());
        var node = new NodeData("Parameters");
        node.Properties["parameters"] = parameters;
        var generator = new MustacheCodeGeneration(provider, services);
        JsonConvert.PopulateObject("""
            {
              "Captures": { "parameters": { "Key": "parameters", "DisableMacro": true } },
              "Head": [ "function(self{{#self.parameters}}, {{{self.parameters}}}{{/self.parameters}})" ],
              "IgnoreChildren": true
            }
            """, generator);
        using var registration = provider.Register("Parameters", CreatePackage(), generator);
        Assert.Equal(expected ? $"function(self, {parameters})" : "function(self)",
            string.Concat(provider.GenerateCode(node, context).Select(x => x.Content)).Trim());
    }

    private static ServiceProvider CreateServices() => new ServiceCollection()
        .AddSingleton<CodeGeneratorServiceProvider>()
        .AddSingleton<LanguageProviderService>()
        .AddSingleton<DefaultValueServiceProvider>()
        .BuildServiceProvider();

    private static PackageInfo CreatePackage() =>
        new(new PackageManifest("test", new Version(1, 0), 0f, null), string.Empty);
}
