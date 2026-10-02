using System.Reflection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Tests;

public sealed class IndexedPackageLookupTests
{
    [Theory]
    [InlineData("LocalVar", "init value", "LocalVariable", "value_0")]
    [InlineData("Assignment", "value", "Assignment", "value_0")]
    [InlineData("RecordPos", "object", "RecordPos", "object_0")]
    public void SerializedSpellingsResolveActualPackageConfiguration(string legacyType,
        string valueCaption, string expectedType, string expectedValueKey)
    {
        using var services = CreateServices();
        using var package = LoadPackage(services);
        var provider = services.GetRequiredService<SharpNodeConverterServiceProvider>();
        foreach (var assemblyName in new[] { "", "LuaSTGEditorSharp" })
        {
            foreach (var banned in new[] { false, true })
            {
                var raw = new SharpNode
                {
                    Type = $".Data.{legacyType}, {assemblyName}",
                    IsBanned = banned,
                    SharpAttributes =
                    [
                        Attribute("Number of Var", "1"),
                        Attribute("Var 1 name", "a"),
                        Attribute($"Var 1 {valueCaption}", "self")
                    ]
                };
                var node = new SharpNodeSerializer().DeserializeTreeNode(JsonConvert.SerializeObject(raw))!;
                Assert.Equal(raw.Type, node.TypeUID);
                var converter = provider.GetConverter(node.TypeUID);
                Assert.Equal(raw.Type, converter.SharpName);
                converter.Convert(node, new());
                Assert.Equal(expectedType, node.TypeUID);
                Assert.Equal("1", node.GetProperty("count"));
                Assert.Equal("a", node.GetProperty("name_0"));
                Assert.Equal("self", node.GetProperty(expectedValueKey));
                Assert.Equal(!banned, node.IsActive);
                Assert.False(node.HasProperty("number_of_var"));
                Assert.DoesNotContain(node.Properties.Keys,
                    key => key.StartsWith("var_", StringComparison.Ordinal));
            }
        }
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("", true)]
    [InlineData("LuaSTGEditorSharp", false)]
    [InlineData("LuaSTGEditorSharp", true)]
    public void RepeatedRecordPosCaptionsReachIndexedMapping(string assemblyName, bool banned)
    {
        using var services = CreateServices();
        using var package = LoadPackage(services);
        var raw = new SharpNode
        {
            Type = $".Data.RecordPos, {assemblyName}",
            IsBanned = banned,
            SharpAttributes =
            [
                Attribute("Number of Var", "2"),
                Attribute("Use var", "p"), Attribute("To record", "self"),
                Attribute("Use var", "q"), Attribute("To record", "p")
            ]
        };
        var node = new SharpNodeSerializer().DeserializeTreeNode(JsonConvert.SerializeObject(raw))!;
        var converter = services.GetRequiredService<SharpNodeConverterServiceProvider>()
            .GetConverter(node.TypeUID);
        Assert.Equal(raw.Type, converter.SharpName);
        converter.Convert(node, new());
        Assert.Equal("RecordPos", node.TypeUID);
        Assert.Equal("2", node.GetProperty("count"));
        Assert.Equal("p", node.GetProperty("name_0"));
        Assert.Equal("self", node.GetProperty("object_0"));
        Assert.Equal("q", node.GetProperty("name_1"));
        Assert.Equal("p", node.GetProperty("object_1"));
        Assert.Equal(!banned, node.IsActive);
        Assert.Equal(new[] { "Number of Var", "Use var", "To record", "Use var", "To record" },
            raw.SharpAttributes!.Select(attribute => attribute!.AttrCap));
    }

    private static SharpAttribute Attribute(string caption, string value)
        => new() { AttrCap = caption, AttrInput = value };

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection()
            .AddSingleton<SerializationTypeBindingService>()
            .AddSingleton<SharpNodeConverterServiceProvider>();
        services.Add(typeof(SharpNodeFormatConverter).GetCustomAttribute<InjectAttribute>()!
            .ToDescriptor(typeof(SharpNodeFormatConverter)));
        return services.BuildServiceProvider();
    }

    private static PackageDescriptor LoadPackage(IServiceProvider services)
    {
        var packed = new PackedServiceCollection();
        packed.Add<SharpNodeConverterServiceProvider>();
        var loader = new NodePackageProvider(NullLogger<NodePackageProvider>.Instance, packed, services);
        return loader.LoadPackageFromDirectory(new PackageAssemblyDescriptor("LegacyNode",
            Path.Combine(AppContext.BaseDirectory, "configs"),
            new PackageManifest("LegacyNode", new Version(1, 0), 2f, null), []));
    }
}
