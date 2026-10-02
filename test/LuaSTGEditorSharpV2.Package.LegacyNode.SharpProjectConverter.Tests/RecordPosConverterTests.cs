using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Tests;

public sealed class RecordPosConverterTests
{
    [Theory]
    [InlineData("RecordPos")]
    [InlineData(".Data.RecordPos, LuaSTGEditorSharp")]
    [InlineData("LuaSTGEditorSharp.EditorData.Node.Data.RecordPos, LuaSTGNodelib")]
    public void DuplicateCaptionsBecomeUniqueKeysBeforeDictionaryConstruction(string type)
    {
        var source = Raw(type, "2", "p", "self", "q", "player");
        var before = JsonConvert.SerializeObject(source);
        var converted = source.ToNodeData();
        Assert.Equal(type, converted.TypeUID);
        Assert.Equal("2", converted.GetProperty("Number of Var"));
        Assert.Equal("p", converted.GetProperty("Var 1 name"));
        Assert.Equal("self", converted.GetProperty("Var 1 object"));
        Assert.Equal("q", converted.GetProperty("Var 2 name"));
        Assert.Equal("player", converted.GetProperty("Var 2 object"));
        Assert.Equal(before, JsonConvert.SerializeObject(source));
    }

    [Fact]
    public void NumberedCaptionsUseTheSamePositionalLayout()
    {
        var source = Raw("RecordPos", "2", "p", "self", "q", "player");
        source.SharpAttributes![1]!.AttrCap = "Use var 1";
        source.SharpAttributes[2]!.AttrCap = "To record 1";
        source.SharpAttributes[3]!.AttrCap = "Use var 2";
        source.SharpAttributes[4]!.AttrCap = "To record 2";
        Assert.Equal("q", source.ToNodeData().GetProperty("Var 2 name"));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("bad")]
    [InlineData("-1")]
    public void InactivePhysicalPairsAndDisabledStateAreRetained(string count)
    {
        var source = Raw("RecordPos", count, "", "self", "q", "player");
        source.IsBanned = true;
        var converted = source.ToNodeData();
        Assert.False(converted.IsActive);
        Assert.Equal("", converted.GetProperty("Var 1 name"));
        Assert.Equal("q", converted.GetProperty("Var 2 name"));
        Assert.Equal("player", converted.GetProperty("Var 2 object"));
    }

    [Fact]
    public void HundredPairsAreSupportedWithoutReadingPastTheArray()
    {
        var values = Enumerable.Range(0, 100).SelectMany(i => new[] { "p" + i, "self" }).ToArray();
        var source = Raw("RecordPos", "101", values);
        Assert.Equal("p99", source.ToNodeData().GetProperty("Var 100 name"));
    }

    [Theory]
    [InlineData("2", new[] { "p", "self" })]
    [InlineData("1", new[] { "p" })]
    public void TruncatedArraysProduceRecordPosDiagnostic(string count, string[] values)
    {
        var source = Raw("RecordPos", count, values);
        var before = JsonConvert.SerializeObject(source);
        var error = Assert.Throws<InvalidOperationException>(() => source.ToNodeData());
        Assert.Contains("RecordPos", error.Message);
        Assert.Equal(before, JsonConvert.SerializeObject(source));
    }

    [Fact]
    public void EmptyAndNullSlotArraysAreDiagnosed()
    {
        var empty = new SharpNode { Type = "RecordPos" };
        Assert.Throws<InvalidOperationException>(() => empty.ToNodeData());
        var source = Raw("RecordPos", "1", "p", "self");
        source.SharpAttributes![1] = null;
        Assert.Throws<InvalidOperationException>(() => source.ToNodeData());
    }

    [Fact]
    public void OtherNodesStillRejectDuplicateCaptions()
    {
        var source = Raw("OtherNode", "2", "p", "self", "q", "player");
        Assert.Throws<ArgumentException>(() => source.ToNodeData());
    }

    private static SharpNode Raw(string type, string count, params string[] values)
    {
        var attributes = new List<SharpAttribute?>
        {
            new() { AttrCap = "Number of Var", AttrInput = count }
        };
        for (var index = 0; index < values.Length; index++)
        {
            attributes.Add(new SharpAttribute
            {
                AttrCap = index % 2 == 0 ? "Use var" : "To record",
                AttrInput = values[index]
            });
        }
        return new SharpNode { Type = type, SharpAttributes = attributes.ToArray() };
    }
}
