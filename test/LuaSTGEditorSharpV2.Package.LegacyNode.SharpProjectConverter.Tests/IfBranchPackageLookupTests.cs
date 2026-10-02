using System.Reflection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Tests;

public sealed class IfBranchPackageLookupTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("", true)]
    [InlineData("LuaSTGEditorSharp", false)]
    [InlineData("LuaSTGEditorSharp", true)]
    public void ElseIfSpellingsResolveActualPackageConfiguration(string assemblyName, bool banned)
    {
        using var services = CreateServices();
        using var package = LoadPackage(services);
        var raw = new SharpNode
        {
            Type = $".General.IfElseIf, {assemblyName}",
            IsBanned = banned,
            SharpAttributes = [new SharpAttribute { AttrCap = "Condition", AttrInput = "x == 1" }]
        };
        var node = new SharpNodeSerializer().DeserializeTreeNode(JsonConvert.SerializeObject(raw))!;
        var child = new NodeData("child") { IsActive = false };
        node.Add(child);
        var converter = services.GetRequiredService<SharpNodeConverterServiceProvider>()
            .GetConverter(node.TypeUID);
        Assert.Equal(raw.Type, converter.SharpName);
        converter.Convert(node, new());
        Assert.Equal("If.ElseIf", node.TypeUID);
        Assert.Equal("x == 1", node.GetProperty("condition"));
        Assert.Equal(!banned, node.IsActive);
        Assert.Same(child, Assert.Single(node.PhysicalChildren));
        Assert.False(child.IsActive);
    }

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
