using System.Reflection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Specialized.Curve;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CodeGenerator.Tests;

public sealed class ObjectCreatorCodeGenerationTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData(" \t\r\n", false)]
    [InlineData("\u2000", false)]
    [InlineData("0", true)]
    [InlineData("false", true)]
    [InlineData("'quoted'", true)]
    [InlineData(" 0 ", true)]
    public void ParametersKeepTheExistingNonWhitespaceSelection(string parameters, bool present)
    {
        using var fixture = new Fixture();
        var node = Creator("Object", parameters);
        var output = fixture.Generate(node);

        Assert.Equal("last = New(_editor_class['Object'], 10, 20" +
            (present ? ", " + parameters : "") + ")\n\n",
            string.Concat(output.Select(code => code.Content)));
        Assert.Equal(new[] { node, node }, SourcesByLine(output));
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(0, 1, false)]
    [InlineData(1, 1, true)]
    public void OnlyActiveChildrenReceiveTheCreatedObjectScope(int active, int disabled, bool wrapped)
    {
        using var fixture = new Fixture();
        var node = Creator("Object");
        NodeData? enabled = null;
        for (var i = 0; i < active + disabled; i++)
        {
            var child = new NodeData("CreatorStatement") { IsActive = i < active };
            node.Add(child);
            if (child.IsActive) enabled = child;
        }
        var output = fixture.Generate(node);

        Assert.Equal("last = New(_editor_class['Object'], 10, 20)\n" +
            (wrapped ? "do\n  local self = last\n  self.x = 1\nend\n" : "\n"),
            string.Concat(output.Select(code => code.Content)));
        Assert.Equal(wrapped
            ? new[] { node, node, node, enabled!, node }
            : new[] { node, node }, SourcesByLine(output));
    }

    [Fact]
    public void NestedCreatorsKeepTheirOwnLocalSelfScope()
    {
        using var fixture = new Fixture();
        var outer = Creator("Outer");
        var inner = Creator("Inner");
        var statement = new NodeData("CreatorStatement");
        outer.Add(inner);
        inner.Add(statement);
        var output = fixture.Generate(outer);

        Assert.Equal("""
            last = New(_editor_class['Outer'], 10, 20)
            do
              local self = last
              last = New(_editor_class['Inner'], 10, 20)
              do
                local self = last
                self.x = 1
              end
            end
            """.Replace("\r\n", "\n") + "\n", string.Concat(output.Select(code => code.Content)));
        Assert.Equal(new[] { outer, outer, outer, inner, inner, inner, statement, inner, outer },
            SourcesByLine(output));
    }

    private static NodeData Creator(string name, string parameters = "")
    {
        var node = new NodeData("CreateObject");
        node.Properties["identifier"] = "'" + name + "'";
        node.Properties["position"] = "10, 20";
        node.Properties["parameters"] = parameters;
        return node;
    }

    private static NodeData[] SourcesByLine(IEnumerable<CodeData> output) =>
        output.SelectMany(code => Enumerable.Repeat(code.Source, code.LineCount)).ToArray();

    private sealed class Fixture : IDisposable
    {
        private static readonly PackageManifest Manifest = new("LegacyNode", new Version(1, 0), 2f, null);
        private readonly List<IDisposable> registrations = [];
        private readonly ServiceProvider services;
        private readonly CodeGeneratorServiceProvider generator;
        public CodeGenerationContext Context { get; }

        public Fixture()
        {
            var collection = new ServiceCollection()
                .AddSingleton<LanguageProviderService>()
                .AddSingleton<SerializationTypeBindingService>()
                .AddSingleton<CodeGeneratorServiceProvider>()
                .AddSingleton<DefaultValueServiceProvider>();
            collection.Add(typeof(DefaultValueService).GetCustomAttribute<InjectAttribute>()!
                .ToDescriptor(typeof(DefaultValueService)));
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
            Context = new(services, new LocalServiceParam(null!),
                new CodeGenerationServiceSettings { IndentionString = "  " });
            var packed = new PackedServiceCollection();
            packed.Add<CodeGeneratorServiceProvider>();
            packed.Add<DefaultValueServiceProvider>();
            var loader = new NodePackageProvider(NullLogger<NodePackageProvider>.Instance, packed, services);
            registrations.Add(loader.LoadPackageFromDirectory(new PackageAssemblyDescriptor(
                "LegacyNode", Path.Combine(AppContext.BaseDirectory, "package", "LegacyNode"), Manifest,
                [typeof(NumericalTrackCodeGeneration).Assembly])));

            var statement = new ConfigurableCodeGeneration(generator, services);
            JsonConvert.PopulateObject("""{"Head":"self.x = 1","IgnoreChildren":true}""", statement);
            registrations.Add(generator.Register("CreatorStatement", new PackageInfo(Manifest, ""), statement));
        }

        public CodeData[] Generate(NodeData node) => generator.GenerateCode(node, Context).ToArray();

        public void Dispose()
        {
            foreach (var registration in registrations.AsEnumerable().Reverse())
            {
                registration.Dispose();
            }
            services.Dispose();
        }
    }
}
