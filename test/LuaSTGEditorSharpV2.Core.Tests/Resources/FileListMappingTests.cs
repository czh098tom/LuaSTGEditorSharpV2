using LuaSTGEditorSharpV2.Core.Resources;
using Newtonsoft.Json;
using Xunit;

namespace LuaSTGEditorSharpV2.Core.Tests.Resources;

public class FileListMappingTests
{
    [Theory]
    [InlineData("0", true, 0)]
    [InlineData("3", true, 3)]
    [InlineData(" 2 ", true, 2)]
    [InlineData("-1", false, -1)]
    [InlineData("", false, 0)]
    [InlineData("invalid", false, 0)]
    [InlineData("2147483648", false, 0)]
    public void CountsMustBeNonnegativeIntegers(string value, bool valid, int expected)
    {
        Assert.Equal(valid, FileListMapping.TryParseCount(value, out var count));
        Assert.Equal(expected, count);
    }

    [Fact]
    public void CountDeterminesContiguousPairsWithoutInspectingPropertyKeys()
    {
        var files = new FileListMapping();
        Assert.Equal("count", files.Count.Key);
        Assert.Equal("0", files.Count.DefaultValue);
        var items = files.GetItems(3);

        Assert.Equal(new[] { 0, 1, 2 }, items.Select(item => item.Index));
        Assert.Equal("source_2", items[2].Source.Key);
        Assert.Equal("destination_2", items[2].Destination.Key);
        Assert.Empty(files.GetItems(0));
    }

    [Fact]
    public void CustomMappingsAndDefaultsArePreserved()
    {
        var files = JsonConvert.DeserializeObject<FileListMapping>("""
            {
              "Source": { "Key": "input_{0}", "DefaultValue": "input.dat" },
              "Destination": { "Key": "output_{0}" }
            }
            """)!.WithCount(NodePropertyCapture.FromKey("length", "0"));

        Assert.Equal("length", files.Count.Key);
        Assert.Equal("input_1", files.GetItems(2)[1].Source.Key);
        Assert.Equal("input.dat", files.GetItems(2)[1].Source.DefaultValue);
        Assert.Equal("output_1", files.GetItems(2)[1].Destination.Key);
    }

    [Theory]
    [InlineData("count", "source", "destination_{0}", 1)]
    [InlineData("source_0", "source_{0}", "destination_{0}", 1)]
    [InlineData("count", "file_{0}", "file_{0}", 1)]
    [InlineData("count", "file_{0}", "file_1{0}", 11)]
    public void OverlappingOrUnindexedMappingsAreRejected(
        string count, string source, string destination, int length)
    {
        var files = JsonConvert.DeserializeObject<FileListMapping>(
            JsonConvert.SerializeObject(new
            {
                Count = new { Key = count },
                Source = new { Key = source },
                Destination = new { Key = destination },
            }))!;

        Assert.Throws<InvalidOperationException>(() => files.GetItems(length));
    }

    [Fact]
    public void NegativeLengthsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FileListMapping().GetItems(-1));
    }
}
