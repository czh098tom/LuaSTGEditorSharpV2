using System.Reflection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Tests;

public sealed class IndexedPackageLookupTests
{
    [Fact]
    public void AllSharpConverterConfigurationsAreRegistered()
    {
        var expected = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "configs"),
                "*.sharpconv", SearchOption.AllDirectories)
            .Select(path => JObject.Parse(File.ReadAllText(path)).Value<string>("SharpName")!)
            .ToArray();
        Assert.NotEmpty(expected);
        Assert.All(expected, name => Assert.False(string.IsNullOrWhiteSpace(name)));
        Assert.Equal(expected.Length, expected.Distinct(StringComparer.Ordinal).Count());

        using var services = CreateServices();
        using var package = LoadPackage(services);
        var provider = services.GetRequiredService<SharpNodeConverterServiceProvider>();
        var registered = provider.GetRegisteredAvailableData();
        Assert.Equal(expected.OrderBy(name => name, StringComparer.Ordinal),
            registered.Keys.OrderBy(name => name, StringComparer.Ordinal));
        foreach (var name in expected)
        {
            var converter = registered[name].data;
            Assert.Equal(name, converter.SharpName);
            Assert.Same(converter, provider.GetConverter(name));
        }
    }

    [Fact]
    public void RecordPosSingleItemMapsLegacyCaptions()
    {
        using var services = CreateServices();
        using var package = LoadPackage(services);
        var raw = new SharpNode
        {
            Type = ".Data.RecordPos, LuaSTGEditorSharp",
            SharpAttributes =
            [
                Attribute("Number of Var", "1"),
                Attribute("Use var", "position"),
                Attribute("To record", "self")
            ]
        };
        var node = new SharpNodeSerializer().DeserializeTreeNode(JsonConvert.SerializeObject(raw))!;
        services.GetRequiredService<SharpNodeConverterServiceProvider>()
            .GetConverter(node.TypeUID).Convert(node, new());

        Assert.Equal("RecordPos", node.TypeUID);
        Assert.Equal(new Dictionary<string, string>
        {
            ["count"] = "1",
            ["name_0"] = "position",
            ["object_0"] = "self"
        }.OrderBy(pair => pair.Key), node.Properties.OrderBy(pair => pair.Key));
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
