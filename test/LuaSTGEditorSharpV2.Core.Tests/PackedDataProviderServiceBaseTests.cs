using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Exception;
using LuaSTGEditorSharpV2.Core.Tests.Stubs;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests;

public class PackedDataProviderServiceBaseTests
{
    private static MinimalPackedDataProvider CreateProvider()
        => new(new ServiceCollection().BuildServiceProvider());

    private static PackageInfo MakeInfo(float priority)
        => new(new PackageManifest("P", new Version("1.0"), priority, null), "");

    [Fact]
    public void Register_LowerPriorityNumber_IsPeekedFirst()
    {
        var provider = CreateProvider();
        var smallerPriority = MakeInfo(1.0f);
        var largerPriority = MakeInfo(5.0f);

        provider.Register("id", largerPriority, "large");
        provider.Register("id", smallerPriority, "small");

        var active = provider.GetRegisteredAvailableData();
        Assert.Equal("small", active["id"].data);
        Assert.Equal(1.0f, active["id"].packageInfo.Manifest.Priority);
    }

    [Fact]
    public void Register_DuplicateData_ThrowsDuplicatedIDException()
    {
        var provider = CreateProvider();
        var packageInfo = MakeInfo(1.0f);
        provider.Register("id1", packageInfo, "same-data");

        Assert.Throws<DuplicatedIDException>(() =>
            provider.Register("id2", packageInfo, "same-data"));
    }

    [Fact]
    public void GetPackageInfo_ReturnsRegisteredManifest()
    {
        var provider = CreateProvider();
        var packageInfo = MakeInfo(2.0f);
        provider.Register("id", packageInfo, "data");

        var result = provider.GetPackageInfo("data");

        Assert.Same(packageInfo, result);
    }
}
