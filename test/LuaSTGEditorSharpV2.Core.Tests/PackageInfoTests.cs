using LuaSTGEditorSharpV2.Core;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests;

public class PackageInfoTests
{
    private static PackageInfo MakeInfo(float priority, string version = "1.0")
        => new(new PackageManifest("P", new Version(version), priority, null), "");

    [Fact]
    public void CompareTo_HigherPriorityNumber_ReturnsPositive()
    {
        var high = MakeInfo(5.0f);
        var low = MakeInfo(1.0f);

        Assert.True(high.CompareTo(low) > 0);
        Assert.True(low.CompareTo(high) < 0);
    }

    [Fact]
    public void CompareTo_SamePriority_FallsBackToVersion()
    {
        var newer = MakeInfo(1.0f, version: "2.0");
        var older = MakeInfo(1.0f, version: "1.0");

        Assert.True(newer.CompareTo(older) > 0);
        Assert.True(older.CompareTo(newer) < 0);
        Assert.Equal(0, newer.CompareTo(newer));
    }

    [Fact]
    public void CompareTo_NullOther_ReturnsPositive()
    {
        var info = MakeInfo(1.0f);

        Assert.True(info.CompareTo(null) > 0);
    }
}
