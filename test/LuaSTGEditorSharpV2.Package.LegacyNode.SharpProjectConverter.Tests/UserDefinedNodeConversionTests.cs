using System.Reflection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Tests;

public sealed class UserDefinedNodeConversionTests
{
    // CMEX16 lines 119 and 137; unused editor metadata is omitted.
    private const string Definition = """
        {
          "$type": ".Advanced.UserDefinedNode, LuaSTGEditorSharp",
          "Attributes": [
            {
              "attrCap": "Name",
              "attrInput": "changeBulletPreBreakColor"
            },
            {
              "attrCap": "Head parse rule",
              "attrInput": "changeBulletPreBreakColor({0}, {1})"
            },
            {
              "attrCap": "Tail parse rule",
              "attrInput": ""
            },
            {
              "attrCap": "Number of prop",
              "attrInput": "2"
            },
            {
              "attrCap": "Prop 1 name",
              "attrInput": "Target"
            },
            {
              "attrCap": "Prop 1 is string",
              "attrInput": "false"
            },
            {
              "attrCap": "Prop 1 edit window",
              "attrInput": "objectDef"
            },
            {
              "attrCap": "Prop 2 name",
              "attrInput": "Color"
            },
            {
              "attrCap": "Prop 2 is string",
              "attrInput": "false"
            },
            {
              "attrCap": "Prop 2 edit window",
              "attrInput": "color"
            }
          ]
        }
        """;

    private const string Instance = """
        {
          "$type": ".Advanced.UnidentifiedNode, LuaSTGEditorSharp",
          "Attributes": [
            {
              "attrCap": "Type",
              "attrInput": "changeBulletPreBreakColor"
            },
            {
              "attrCap": "Target",
              "attrInput": "self"
            },
            {
              "attrCap": "Color",
              "attrInput": "COLOR_GREEN"
            }
          ]
        }
        """;

    [Fact]
    public void InstanceUsesDefinitionCapturedBeforeFormatting()
    {
        using var services = CreateServices();
        using var package = LoadPackage(services);
        var provider = services.GetRequiredService<SharpNodeConverterServiceProvider>();
        var serializer = new SharpNodeSerializer();
        var instance = serializer.DeserializeTreeNode(Instance)!;
        var definition = serializer.DeserializeTreeNode(Definition)!;
        var child = new NodeData("child");
        instance.Add(child);
        var root = new NodeData("root");
        root.Add(definition);
        root.Add(instance);
        var context = new SharpNodeFormattingContext(root, string.Empty, _ => { });

        provider.GetConverter(definition.TypeUID).Convert(definition, context);
        provider.GetConverter(instance.TypeUID).Convert(instance, context);

        Assert.Equal("CodeSegment", instance.TypeUID);
        Assert.Equal(new Dictionary<string, string>
        {
            ["head"] = "changeBulletPreBreakColor(self, COLOR_GREEN)",
            ["tail"] = ""
        }.OrderBy(pair => pair.Key), instance.Properties.OrderBy(pair => pair.Key));
        Assert.Same(child, Assert.Single(instance.PhysicalChildren));
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
