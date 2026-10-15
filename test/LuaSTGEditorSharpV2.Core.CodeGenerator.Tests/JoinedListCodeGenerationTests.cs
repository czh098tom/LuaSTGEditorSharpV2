using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Tests;

public sealed class JoinedListCodeGenerationTests
{
    private const string Assignment = """
        {"Language":"Probe","CountCapture":{"Key":"count"},"IgnoreChildren":true,
         "ItemCaptures":{"name":{"Key":"name_{0}","DisableMacro":true},"value":{"Key":"value_{0}"}},
         "RequiredCaptureName":"name",
         "Columns":{"names":{"Template":["{{{args.item.name}}}"],"Separator":","},
                    "values":{"Template":["({{{args.item.value}}})"],"Separator":","}},
         "Head":["{{#args.hasItems}}{{{args.columns.names}}}={{{args.columns.values}}}{{/args.hasItems}}"]}
        """;

    [Fact]
    public void AssignmentRemainsOneParallelStatementAndParenthesizesEachRhs()
    {
        using var fixture = new Fixture();
        var generator = fixture.Generator(Assignment);
        var node = Node("2", "a", "b", "b", "a");
        Assert.Equal("a,b=(b),(a)", fixture.Render(generator, node));
        node.Properties["value_1"] = "f()";
        Assert.Equal("a,b=(b),(f())", fixture.Render(generator, node));
    }

    [Fact]
    public void AllColumnsShareOneEmptyNameFilterWithoutCompactingPhysicalSlots()
    {
        using var fixture = new Fixture();
        var node = Node("3", "", "ignored()", "a", "f()", "", "unused()");
        Assert.Equal("a=(f())", fixture.Render(fixture.Generator(Assignment), node));
        Assert.Equal("3", node.GetProperty("count"));
        Assert.Equal("a", node.GetProperty("name_1"));
        Assert.Equal("ignored()", node.GetProperty("value_0"));
    }

    [Fact]
    public void WhitespaceNamesAreNotSilentlyRemoved()
    {
        using var fixture = new Fixture();
        var node = Node("1", " ", "1");
        Assert.Equal(" =(1)", fixture.Render(fixture.Generator(Assignment), node));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("bad")]
    public void EmptyRowsDoNotEmitIncompleteAssignments(string count)
    {
        using var fixture = new Fixture();
        Assert.Equal("", fixture.Render(fixture.Generator(Assignment), Node(count)));
    }

    [Fact]
    public void MissingCountUsesCaptureDefaultAndMaximumCountCapsIteration()
    {
        using var fixture = new Fixture();
        var config = JObject.Parse(Assignment);
        config["MaximumCount"] = 1;
        config["CountCapture"]!["DefaultValue"] = "2";
        var node = Node("100", "a", "1", "b", "2");
        var generator = fixture.Generator(config.ToString());
        Assert.Equal("a=(1)", fixture.Render(generator, node));
        node.Properties.Remove("count");
        Assert.Equal("a=(1)", fixture.Render(generator, node));
    }

    [Fact]
    public void MacroExpansionIsPerRhsAndNeverTouchesNamesOrJoinedText()
    {
        using var fixture = new Fixture();
        var generator = fixture.Generator(Assignment);
        var node = Node("2", "old", "old", "b", "old");
        var macro = new NodeData("macro");
        macro.Properties["replace"] = "old";
        macro.Properties["by"] = "new";
        using (fixture.Context.AcquireContextHandle(macro))
        {
            Assert.Equal("old,b=(new),(new)", fixture.Render(generator, node));
        }
        // Matching across joined rows must not run after column assembly.
        macro.Properties["replace"] = @"old\),\(old";
        macro.Properties["by"] = "crossed";
        using (fixture.Context.AcquireContextHandle(macro))
        {
            Assert.Equal("old,b=(old),(old)", fixture.Render(generator, node));
        }
    }

    [Fact]
    public void DisabledMacroCapturesRemainRawOnBothSidesOfRecordPos()
    {
        using var fixture = new Fixture();
        var config = JObject.Parse(Assignment);
        config["ItemCaptures"]!["value"]!["DisableMacro"] = true;
        config["Columns"]!["values"]!["Template"] = new JArray("{ x={{{args.item.value}}}.x ,y={{{args.item.value}}}.y }");
        config["Head"] = new JArray("{{#args.hasItems}}local {{{args.columns.names}}}={{{args.columns.values}}}{{/args.hasItems}}");
        var node = Node("2", "old", "old", "q", "player");
        var macro = new NodeData("macro");
        macro.Properties["replace"] = "old";
        macro.Properties["by"] = "new";
        using (fixture.Context.AcquireContextHandle(macro))
        {
            Assert.Equal("local old,q={ x=old.x ,y=old.y },{ x=player.x ,y=player.y }",
                fixture.Render(fixture.Generator(config.ToString()), node));
        }
    }

    [Fact]
    public void LocalDeclarationsRemainSequentialWithOptionalParenthesizedValues()
    {
        using var fixture = new Fixture();
        var config = JObject.Parse(Assignment);
        config["Columns"] = JObject.Parse("""
            {"declarations":{"Template":["local {{{args.item.name}}}{{#args.rawItem.value}}=({{{args.item.value}}}){{/args.rawItem.value}}"],"Separator":"\n"}}
            """);
        config["Head"] = new JArray("{{#args.hasItems}}{{{args.columns.declarations}}}{{/args.hasItems}}");
        var node = Node("3", "a", "", "b", "a", "", "ignored()");
        Assert.Equal("local a\nlocal b=(a)", fixture.Render(fixture.Generator(config.ToString()), node));
    }

    [Fact]
    public void RawInitializationPresenceIsIndependentOfMacroExpandedValue()
    {
        using var fixture = new Fixture();
        var config = JObject.Parse(Assignment);
        config["Columns"] = JObject.Parse("""
            {"declarations":{"Template":["local {{{args.item.name}}}{{#args.rawItem.value}}=({{{args.item.value}}}){{/args.rawItem.value}}"],"Separator":"\n"}}
            """);
        config["Head"] = new JArray("{{{args.columns.declarations}}}");
        var macro = new NodeData("macro");
        macro.Properties["replace"] = "old";
        macro.Properties["by"] = "";
        using (fixture.Context.AcquireContextHandle(macro))
        {
            Assert.Equal("local a=()", fixture.Render(fixture.Generator(config.ToString()), Node("1", "a", "old")));
        }
    }

    [Fact]
    public void PhysicalIndexIsAvailableToTemplatesAndSeparatorsHaveNoImplicitNewlines()
    {
        using var fixture = new Fixture();
        var config = JObject.Parse(Assignment);
        config["Columns"]!["names"]!["Template"] = new JArray("{{args.index}}:{{{args.item.name}}}");
        config["Columns"]!["names"]!["Separator"] = "|";
        config["Head"] = new JArray("{{{args.columns.names}}}");
        Assert.Equal("1:a|2:b", fixture.Render(fixture.Generator(config.ToString()),
            Node("3", "", "", "a", "1", "b", "2")));
    }

    [Fact]
    public void IndentationIsAppliedOnceToEachRenderedLine()
    {
        using var fixture = new Fixture();
        var node = Node("2", "a", "1", "b", "2");
        using (fixture.Context.AcquireContextHandle(new NodeData("Folder"), 2))
        {
            var output = fixture.Render(fixture.Generator(Assignment), node);
            Assert.Equal("\t\ta,b=(1),(2)", output);
        }
    }

    [Fact]
    public void RecursiveCallsKeepParentColumnsAndSkipDisabledChildren()
    {
        using var fixture = new Fixture();
        var config = JObject.Parse(Assignment);
        config["IgnoreChildren"] = false;
        config["Head"] = new JArray("H{{{args.columns.names}}}:{{logicalChildrenCount}}");
        config["Tail"] = new JArray("T{{{args.columns.names}}}:{{logicalChildrenCount}}");
        var generator = fixture.Generator(config.ToString());
        var parent = Node("1", "parent", "");
        var child = Node("1", "child", "");
        var disabled = Node("1", "disabled", "");
        disabled.IsActive = false;
        parent.Add(child);
        parent.Add(disabled);
        using var registration = fixture.Provider.Register("List", fixture.Package, generator);
        Assert.Equal(new[] { "Hparent:1", "Hchild:0", "Tchild:0", "Tparent:1" },
            fixture.Provider.GenerateCode(parent, fixture.Context).Select(code => code.Content.TrimEnd('\r', '\n')));
    }

    [Fact]
    public void MissingRequiredCaptureReportsConfigurationError()
    {
        using var fixture = new Fixture();
        var config = JObject.Parse(Assignment);
        config["RequiredCaptureName"] = "absent";
        Assert.Throws<InvalidOperationException>(() => fixture.Render(fixture.Generator(config.ToString()), Node("1")));
    }

    private static NodeData Node(string count, params string[] values)
    {
        var node = new NodeData("List");
        node.Properties["count"] = count;
        for (var index = 0; index < values.Length; index += 2)
        {
            node.Properties["name_" + index / 2] = values[index];
            node.Properties["value_" + index / 2] = values[index + 1];
        }
        return node;
    }

    private sealed class ProbeLanguage(IServiceProvider services) : LanguageBase(services)
    {
        public override string Name => "Probe";
        public void EnableMacro() => MacroTemplate = "{0}";
    }

    private sealed class Fixture : IDisposable
    {
        private readonly ServiceProvider _services = new ServiceCollection()
            .AddSingleton<CodeGeneratorServiceProvider>()
            .AddSingleton<LanguageProviderService>()
            .AddSingleton<DefaultValueServiceProvider>()
            .BuildServiceProvider();
        private readonly IDisposable _languageRegistration;
        public CodeGeneratorServiceProvider Provider { get; }
        public CodeGenerationContext Context { get; }
        public PackageInfo Package { get; } = new(new PackageManifest("test", new Version(1, 0), 0f, null), string.Empty);

        public Fixture()
        {
            Provider = _services.GetRequiredService<CodeGeneratorServiceProvider>();
            Context = new(_services, new LocalServiceParam(null!), new CodeGenerationServiceSettings());
            var language = new ProbeLanguage(_services);
            language.EnableMacro();
            _languageRegistration = _services.GetRequiredService<LanguageProviderService>().Register("Probe", Package, language);
        }

        public JoinedListCodeGeneration Generator(string json)
        {
            var generator = new JoinedListCodeGeneration(Provider, _services);
            JsonConvert.PopulateObject(json, generator);
            return generator;
        }

        public string Render(JoinedListCodeGeneration generator, NodeData node)
        {
            using var registration = Provider.Register("List", Package, generator);
            return string.Concat(Provider.GenerateCode(node, Context).Select(code => code.Content)).TrimEnd('\r', '\n');
        }

        public void Dispose()
        {
            _languageRegistration.Dispose();
            _services.Dispose();
        }
    }
}
