using LuaSTGEditorSharpV2.Core.Building;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;
using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Resources;

public class FileListResourceGatheringTests
{
    [Fact]
    public void GatheringPreservesSourceDestinationPairsAndItemOrder()
    {
        using var fixture = new Fixture();
        var node = CreateNode(("assets/a.png", "boss/a.png"), ("sound/b.ogg", "audio/b.ogg"));
        fixture.Context.PushResourceGroup("default");

        var resources = fixture.Service.GetResourcesToPackWithContext(node, fixture.Context).ToArray();

        Assert.Equal(
            new[]
            {
                new GroupedResource("assets/a.png", "boss/a.png", "default"),
                new GroupedResource("sound/b.ogg", "audio/b.ogg", "default"),
            },
            resources);
    }

    [Fact]
    public void EveryActiveGroupReceivesEveryItem()
    {
        using var fixture = new Fixture();
        fixture.Context.PushResourceGroup("outer");
        fixture.Context.PushResourceGroup("inner");

        var resources = fixture.Service.GetResourcesToPackWithContext(
            CreateNode(("source.lua", "target.lua")), fixture.Context).ToArray();

        Assert.Equal(new[] { "inner", "outer" }, resources.Select(resource => resource.ResourceGroup));
        Assert.All(resources, resource => Assert.Equal("target.lua", resource.TargetName));
    }

    [Fact]
    public void EmptyGroupStackDoesNotInventADefaultGroup()
    {
        using var fixture = new Fixture();

        Assert.Empty(fixture.Service.GetResourcesToPackWithContext(
            CreateNode(("source.lua", "target.lua")), fixture.Context));
    }

    [Theory]
    [InlineData("", "target.lua")]
    [InlineData("source.lua", "")]
    public void MissingPairMemberIsReportedInsteadOfFallingBack(string source, string destination)
    {
        using var fixture = new Fixture();
        fixture.Context.PushResourceGroup("default");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.Service.GetResourcesToPackWithContext(
                CreateNode((source, destination)), fixture.Context).ToArray());

        Assert.Contains("file 0", exception.Message);
        Assert.Contains("FileListTest", exception.Message);
    }

    [Fact]
    public void AllPairsAreValidatedBeforeAnyResourceIsYielded()
    {
        using var fixture = new Fixture();
        fixture.Context.PushResourceGroup("default");
        var node = CreateNode(("first.lua", "first.lua"), ("second.lua", ""));
        using var enumerator = fixture.Service.GetResourcesToPackWithContext(
            node, fixture.Context).GetEnumerator();

        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
    }

    [Fact]
    public void CustomMappingIsSharedWithTheEditorContract()
    {
        using var fixture = new Fixture();
        JsonConvert.PopulateObject("""
            {
              "Files": {
                "Count": { "Key": "length", "DefaultValue": "0" },
                "Source": { "Key": "input_{0}" },
                "Destination": { "Key": "output_{0}" }
              }
            }
            """, fixture.Service);
        var node = new NodeData("FileListTest");
        node.Properties["length"] = "1";
        node.Properties["input_0"] = "input.dat";
        node.Properties["output_0"] = "data/output.dat";
        fixture.Context.PushResourceGroup("default");

        Assert.Equal(
            new GroupedResource("input.dat", "data/output.dat", "default"),
            Assert.Single(fixture.Service.GetResourcesToPackWithContext(node, fixture.Context)));
    }

    [Fact]
    public void EmptyListStillTraversesChildNodes()
    {
        using var fixture = new Fixture();
        using var registration = fixture.Provider.Register(
            "FileListTest", new PackageInfo(PackageManifest.CORE, string.Empty), fixture.Service);
        var parent = CreateNode();
        parent.Add(CreateNode(("child.lua", "scripts/child.lua")));
        fixture.Context.PushResourceGroup("default");

        Assert.Equal(
            new GroupedResource("child.lua", "scripts/child.lua", "default"),
            Assert.Single(fixture.Service.GetResourcesToPackWithContext(parent, fixture.Context)));
    }

    [Fact]
    public void CountIsAuthoritativeAndOutOfRangeKeysAreNotDiscovered()
    {
        using var fixture = new Fixture();
        var node = CreateNode(("first.lua", "first.lua"));
        node.Properties["source_9"] = "last.lua";
        node.Properties["destination_9"] = "scripts/last.lua";
        fixture.Context.PushResourceGroup("default");

        Assert.Equal("first.lua", Assert.Single(fixture.Service.GetResourcesToPackWithContext(
            node, fixture.Context)).TargetName);
    }

    [Fact]
    public void MissingCountUsesZeroWithoutInferringTheList()
    {
        using var fixture = new Fixture();
        var node = CreateNode(("first.lua", "first.lua"));
        node.Properties.Remove("count");
        fixture.Context.PushResourceGroup("default");

        Assert.Empty(fixture.Service.GetResourcesToPackWithContext(node, fixture.Context));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("2147483648")]
    public void InvalidCountIsReported(string count)
    {
        using var fixture = new Fixture();
        var node = CreateNode();
        node.Properties["count"] = count;

        Assert.Throws<InvalidOperationException>(() =>
            fixture.Service.GetResourcesToPackWithContext(node, fixture.Context).ToArray());
    }

    [Fact]
    public void MissingIndexedMemberIsNotSilentlySkipped()
    {
        using var fixture = new Fixture();
        var node = CreateNode(("one.lua", "one.lua"), ("two.lua", "two.lua"));
        node.Properties.Remove("source_1");
        fixture.Context.PushResourceGroup("default");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.Service.GetResourcesToPackWithContext(node, fixture.Context).ToArray());
        Assert.Contains("file 1", exception.Message);
    }

    [Fact]
    public void DuplicateSourcesAndTargetsRemainSeparateEntries()
    {
        using var fixture = new Fixture();
        fixture.Context.PushResourceGroup("default");

        var resources = fixture.Service.GetResourcesToPackWithContext(
            CreateNode(("same.lua", "same.lua"), ("same.lua", "same.lua")),
            fixture.Context).ToArray();

        Assert.Equal(2, resources.Length);
        Assert.Equal(resources[0], resources[1]);
    }

    private static NodeData CreateNode(params (string Source, string Destination)[] files)
    {
        var node = new NodeData("FileListTest");
        node.Properties["count"] = files.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
        for (var index = 0; index < files.Length; index++)
        {
            node.Properties[$"source_{index}"] = files[index].Source;
            node.Properties[$"destination_{index}"] = files[index].Destination;
        }
        return node;
    }

    private sealed class Fixture : IDisposable
    {
        private readonly ServiceProvider _services;
        public ResourceGatheringServiceProvider Provider { get; }
        public ResourceGatheringContext Context { get; }
        public ConfigurableFileListResourceGatheringService Service { get; }

        public Fixture()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<DefaultValueServiceProvider>();
            services.AddSingleton<ResourceGatheringServiceProvider>();
            _services = services.BuildServiceProvider();
            Provider = _services.GetRequiredService<ResourceGatheringServiceProvider>();
            Context = new ResourceGatheringContext(
                _services, new LocalServiceParam(DocumentModel.Empty), new ResourceGatheringServiceSettings());
            Service = new ConfigurableFileListResourceGatheringService(Provider, _services);
        }

        public void Dispose()
        {
            _services.Dispose();
        }
    }
}
