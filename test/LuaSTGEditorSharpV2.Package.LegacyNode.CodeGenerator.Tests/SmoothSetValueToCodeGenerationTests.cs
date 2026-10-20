using System.Reflection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Tests;

public sealed class SmoothSetValueToCodeGenerationTests
{
    [Theory]
    [InlineData(false, "4", "12", "4", "12")]
    [InlineData(true, "4", "12", "4", "12")]
    [InlineData(false, "0", "0", "0", "0")]
    [InlineData(true, "0", "0", "0", "0")]
    [InlineData(false, "", "", "0", "1")]
    [InlineData(true, "", "", "0", "1")]
    [InlineData(false, " ", " \t", " ", " \t")]
    [InlineData(true, " ", " \t", " ", " \t")]
    [InlineData(false, null, null, "0", "60")]
    [InlineData(true, null, null, "0", "60")]
    [InlineData(false, "", null, "0", "60")]
    [InlineData(true, "", null, "0", "60")]
    [InlineData(false, null, "", "0", "1")]
    [InlineData(true, null, "", "0", "1")]
    public void TimeValuesRoundtripAndGenerateWithoutNormalizingStoredProperties(
        bool imported, string? delay, string? duration, string expectedDelay, string expectedDuration)
    {
        using var fixture = new Fixture();
        var node = Node(imported, "1", delay, duration);
        Item(node, imported, 0, "alpha", "2");
        fixture.ImportIfNeeded(node, imported);

        AssertTimeProperty(node, "delay", delay);
        AssertTimeProperty(node, "duration", duration);
        node = JsonConvert.DeserializeObject<NodeData>(JsonConvert.SerializeObject(node))!;
        var before = JsonConvert.SerializeObject(node);

        Assert.Equal(
            $"lasttask = task.New(self,function()ex.SmoothSetValueTo(\"alpha\",2,{expectedDuration},MOVE_NORMAL,nil,{expectedDelay},MODE_SET)end)\n\n",
            fixture.Generate(node));
        Assert.Equal(before, JsonConvert.SerializeObject(node));
        AssertTimeProperty(node, "delay", delay);
        AssertTimeProperty(node, "duration", duration);
    }

    [Fact]
    public void NativeDefaultNodeStillUsesSixtyFrames()
    {
        using var fixture = new Fixture();
        var node = new NodeData("SmoothSetValueTo");

        Assert.Equal(
            "lasttask = task.New(self,function()ex.SmoothSetValueTo(_speed,1,60,MOVE_NORMAL,nil,0,MODE_SET)end)\n\n",
            fixture.Generate(node));
        Assert.Empty(node.Properties);
    }

    [Theory]
    [InlineData(false, "", "0", "1")]
    [InlineData(true, "", "0", "1")]
    [InlineData(false, " ", " ", " ")]
    [InlineData(true, " ", " ", " ")]
    [InlineData(false, "8", "8", "8")]
    [InlineData(true, "8", "8", "8")]
    public void TimeFallbackRunsAfterMacroExpansion(
        bool imported, string replacement, string expectedDelay, string expectedDuration)
    {
        using var fixture = new Fixture();
        var node = Node(imported, "1", "TOKEN", "TOKEN");
        Item(node, imported, 0, "alpha", "2");
        fixture.ImportIfNeeded(node, imported);
        using var scope = fixture.Context.AcquireContextHandle(Macro("TOKEN", replacement));

        Assert.Equal(
            $"lasttask = task.New(self,function()ex.SmoothSetValueTo(\"alpha\",2,{expectedDuration},MOVE_NORMAL,nil,{expectedDelay},MODE_SET)end)\n\n",
            fixture.Generate(node));
        Assert.Equal("TOKEN", node.GetProperty("delay"));
        Assert.Equal("TOKEN", node.GetProperty("duration"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RowsKeepPhysicalOrderSkipEmptyTargetsAndLeaveInactiveSlotsUnemitted(bool imported)
    {
        using var fixture = new Fixture();
        var node = Node(imported, "4", "", "");
        Item(node, imported, 0, "first", "5", "MOVE_FIRST", "MODE_FIRST");
        Item(node, imported, 1, "skipped", "");
        Item(node, imported, 2, "space", " ", "MOVE_SPACE", "MODE_SPACE");
        Item(node, imported, 3, "last", "9", "MOVE_LAST", "MODE_LAST");
        Item(node, imported, 4, "inactive", "99");
        fixture.ImportIfNeeded(node, imported);
        var before = JsonConvert.SerializeObject(node);

        Assert.Equal(
            "lasttask = task.New(self,function()ex.SmoothSetValueTo(\"first\",5,1,MOVE_FIRST,nil,0,MODE_FIRST)end)\n" +
            "lasttask = task.New(self,function()ex.SmoothSetValueTo(\"space\", ,1,MOVE_SPACE,nil,0,MODE_SPACE)end)\n" +
            "lasttask = task.New(self,function()ex.SmoothSetValueTo(\"last\",9,1,MOVE_LAST,nil,0,MODE_LAST)end)\n\n",
            fixture.Generate(node));
        Assert.Equal(before, JsonConvert.SerializeObject(node));
        Assert.Equal("99", node.GetProperty("target_value4"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RowExpressionsExpandBeforeFilteringWhilePropertyNamesStayLiteral(bool imported)
    {
        using var fixture = new Fixture();
        var node = Node(imported, "2", "0", "3");
        Item(node, imported, 0, "skipped", "TOKEN");
        Item(node, imported, 1, "TOKEN", "2", "MOVE_TOKEN", "MODE_TOKEN");
        fixture.ImportIfNeeded(node, imported);
        using var scope = fixture.Context.AcquireContextHandle(Macro("TOKEN", ""), 2);

        Assert.Equal(
            "  lasttask = task.New(self,function()ex.SmoothSetValueTo(\"TOKEN\",2,3,MOVE_TOKEN,nil,0,MODE_TOKEN)end)\n\n",
            fixture.Generate(node));
        Assert.Equal("\"TOKEN\"", node.GetProperty("variable_name1"));
        Assert.Equal("TOKEN", node.GetProperty("target_value0"));
    }

    [Theory]
    [InlineData(false, "0")]
    [InlineData(true, "0")]
    [InlineData(false, "-1")]
    [InlineData(true, "-1")]
    [InlineData(false, "bad")]
    [InlineData(true, "bad")]
    public void InvalidOrEmptyCountsDoNotEmitTasks(bool imported, string count)
    {
        using var fixture = new Fixture();
        var node = Node(imported, count, "", "");
        Item(node, imported, 0, "unused", "2");
        fixture.ImportIfNeeded(node, imported);

        Assert.Equal("\n", fixture.Generate(node));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MaximumCountExcludesTheHundredAndFirstSlot(bool imported)
    {
        using var fixture = new Fixture();
        var node = Node(imported, "101", "0", "3");
        for (var index = 0; index < 100; index++)
        {
            Item(node, imported, index, "skipped", "");
        }
        Item(node, imported, 99, "hundredth", "2");
        Item(node, imported, 100, "outside", "999");
        fixture.ImportIfNeeded(node, imported);

        Assert.Equal(
            "lasttask = task.New(self,function()ex.SmoothSetValueTo(\"hundredth\",2,3,MOVE_NORMAL,nil,0,MODE_SET)end)\n\n",
            fixture.Generate(node));
    }

    private static void AssertTimeProperty(NodeData node, string key, string? value)
    {
        Assert.Equal(value is not null, node.HasProperty(key));
        if (value is not null)
        {
            Assert.Equal(value, node.GetProperty(key));
        }
    }

    private static NodeData Node(bool imported, string count, string? delay, string? duration)
    {
        var node = new NodeData(imported ? ".Task.SmoothSetValueTo, " : "SmoothSetValueTo");
        node.Properties[imported ? "Number of Value" : "count"] = count;
        if (delay is not null)
        {
            node.Properties[imported ? "Waiting time" : "delay"] = delay;
        }
        if (duration is not null)
        {
            node.Properties[imported ? "Duration" : "duration"] = duration;
        }
        return node;
    }

    private static void Item(NodeData node, bool imported, int index, string name, string value,
        string interpolation = "MOVE_NORMAL", string modification = "MODE_SET")
    {
        if (imported)
        {
            node.Properties[$"Value {index + 1} name"] = name;
            node.Properties[$"Target value {index + 1}"] = value;
            node.Properties[$"Interpolation mode {index + 1}"] = interpolation;
            node.Properties[$"Modification mode {index + 1}"] = modification;
        }
        else
        {
            node.Properties[$"variable_name{index}"] = $"\"{name}\"";
            node.Properties[$"target_value{index}"] = value;
            node.Properties[$"interpolation_mode{index}"] = interpolation;
            node.Properties[$"modification_mode{index}"] = modification;
        }
    }

    private static NodeData Macro(string replace, string by)
    {
        var macro = new NodeData("macro");
        macro.Properties["replace"] = replace;
        macro.Properties["by"] = by;
        return macro;
    }

    private sealed class Fixture : IDisposable
    {
        private static readonly PackageManifest Manifest = new("LegacyNode", new Version(1, 0), 2f, null);
        private readonly List<IDisposable> registrations = [];
        private readonly ServiceProvider services;
        private readonly CodeGeneratorServiceProvider generator;
        private readonly SharpNodeConverterServiceProvider converters;
        public CodeGenerationContext Context { get; }

        public Fixture()
        {
            var collection = new ServiceCollection()
                .AddSingleton<LanguageProviderService>()
                .AddSingleton<SerializationTypeBindingService>()
                .AddSingleton<CodeGeneratorServiceProvider>()
                .AddSingleton<DefaultValueServiceProvider>()
                .AddSingleton<SharpNodeConverterServiceProvider>();
            collection.Add(typeof(DefaultValueService).GetCustomAttribute<InjectAttribute>()!.ToDescriptor(typeof(DefaultValueService)));
            collection.Add(typeof(SharpNodeFormatConverter).GetCustomAttribute<InjectAttribute>()!
                .ToDescriptor(typeof(SharpNodeFormatConverter)));
            foreach (var type in new[]
            {
                typeof(CodeGeneratorServiceBase).Assembly, typeof(NumericalTrackCodeGeneration).Assembly
            }.SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && typeof(CodeGeneratorServiceBase).IsAssignableFrom(type)))
            {
                collection.Add(type.GetCustomAttribute<InjectAttribute>()!.ToDescriptor(type));
            }
            services = collection.BuildServiceProvider();
            generator = services.GetRequiredService<CodeGeneratorServiceProvider>();
            converters = services.GetRequiredService<SharpNodeConverterServiceProvider>();
            Context = new(services, new LocalServiceParam(null!),
                new CodeGenerationServiceSettings { IndentionString = " " });
            registrations.Add(services.GetRequiredService<LanguageProviderService>()
                .Register("Lua", new PackageInfo(Manifest, ""), new TestLanguage(services)));
            var packed = new PackedServiceCollection();
            packed.Add<CodeGeneratorServiceProvider>();
            packed.Add<DefaultValueServiceProvider>();
            packed.Add<SharpNodeConverterServiceProvider>();
            var loader = new NodePackageProvider(NullLogger<NodePackageProvider>.Instance, packed, services);
            registrations.Add(loader.LoadPackageFromDirectory(new PackageAssemblyDescriptor(
                "LegacyNode", Path.Combine(AppContext.BaseDirectory, "package", "LegacyNode"), Manifest,
                [typeof(NumericalTrackCodeGeneration).Assembly, typeof(SharpNodeFormatConverter).Assembly])));
        }

        public void ImportIfNeeded(NodeData node, bool imported)
        {
            if (imported)
            {
                converters.GetConverter(".Task.SmoothSetValueTo, ").Convert(node, new());
                Assert.Equal("SmoothSetValueTo", node.TypeUID);
            }
        }

        public string Generate(NodeData node)
            => string.Concat(generator.GenerateCode(node, Context).Select(code => code.Content));

        public void Dispose()
        {
            foreach (var registration in registrations.AsEnumerable().Reverse())
            {
                registration.Dispose();
            }
            services.Dispose();
        }
    }

    private sealed class TestLanguage : LanguageBase
    {
        public override string Name => "Lua";
        public TestLanguage(IServiceProvider services) : base(services)
        {
            MacroTemplate = @"\b{0}\b";
        }
    }
}
