using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Tests;

public sealed class IndexedListRemappingTests
{
    private const string Mapping = """
        {"OriginalCountKey":"old_count","NewCountKey":"count","MaximumCount":100,
         "PropertyRules":[
           {"OriginalKeyFormat":"var_{0}_name","ItemKey":"name","NewKeyFormat":"name_{0}"},
           {"OriginalKeyFormat":"var_{0}_value","ItemKey":"value","NewKeyFormat":"value_{0}"}]}
        """;

    [Theory]
    [InlineData(null, "0")]
    [InlineData("", "0")]
    [InlineData("bad", "0")]
    [InlineData("-1", "0")]
    [InlineData("0", "0")]
    [InlineData("1", "1")]
    [InlineData("2", "2")]
    [InlineData("100", "100")]
    [InlineData("101", "100")]
    [InlineData("2147483648", "0")]
    public void CountIsBoundedAndStringValued(string? input, string expected)
    {
        var node = new NodeData("List");
        if (input is not null) node.Properties["old_count"] = input;
        var context = new SharpNodeFormattingContext();
        var result = Create().Convert(node, context);
        Assert.Same(node, result);
        Assert.Equal(expected, node.GetProperty("count"));
        Assert.False(node.HasProperty("old_count"));
        Assert.False(context.ShouldRetry);
    }

    [Fact]
    public void EmptyNamesAreNotCompactedAndUnrelatedDigitsAreUntouched()
    {
        var node = Node("2", ("var_1_name", ""), ("var_1_value", "nil"),
            ("var_2_name", "sty"), ("var_2_value", "arrow_big"), ("texture16", "same"));
        Create().Convert(node, new());
        Assert.Equal("", node.GetProperty("name_0"));
        Assert.Equal("nil", node.GetProperty("value_0"));
        Assert.Equal("sty", node.GetProperty("name_1"));
        Assert.Equal("arrow_big", node.GetProperty("value_1"));
        Assert.Equal("same", node.GetProperty("texture16"));
        Assert.False(node.HasProperty("var_2_name"));
    }

    [Fact]
    public void MissingDefaultsAreDistinctFromExplicitEmptyDefaults()
    {
        var config = JObject.Parse(Mapping);
        config["PropertyRules"]![1]!["DefaultValue"] = "";
        var node = Node("1");
        Create(config).Convert(node, new());
        Assert.False(node.HasProperty("name_0"));
        Assert.True(node.HasProperty("value_0"));
        Assert.Equal("", node.GetProperty("value_0"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InactiveSlotsOnlyMoveWhenConfigured(bool preserve)
    {
        var node = Node("1", ("var_1_name", "a"), ("var_2_name", "b"));
        var config = JObject.Parse(Mapping);
        config["PreserveInactiveItems"] = preserve;
        config["PropertyRules"]![1]!["DefaultValue"] = "nil";
        Create(config).Convert(node, new());
        Assert.Equal("nil", node.GetProperty("value_0"));
        Assert.Equal(preserve, node.HasProperty("name_1"));
        Assert.Equal(!preserve, node.HasProperty("var_2_name"));
        Assert.False(node.HasProperty("value_1"));
        Assert.False(node.HasProperty("value_99"));
    }

    [Fact]
    public void InPlaceReindexingUsesSnapshotInsteadOfOverwritingLaterSources()
    {
        var config = JObject.Parse(Mapping);
        config["OriginalCountKey"] = "count";
        config["OriginalIndexBase"] = 0;
        config["NewIndexBase"] = 1;
        config["PropertyRules"] = JArray.Parse("""
            [{"OriginalKeyFormat":"name_{0}","ItemKey":"name","NewKeyFormat":"name_{0}"}]
            """);
        var node = new NodeData("List");
        node.Properties["count"] = "2";
        node.Properties["name_0"] = "a";
        node.Properties["name_1"] = "b";
        Create(config).Convert(node, new());
        Assert.False(node.HasProperty("name_0"));
        Assert.Equal("a", node.GetProperty("name_1"));
        Assert.Equal("b", node.GetProperty("name_2"));
    }

    [Fact]
    public void ExistingTargetCollisionDoesNotPartiallyMutateSource()
    {
        var node = Node("2", ("var_1_name", "a"), ("var_2_name", "b"), ("name_1", "user"));
        var before = JsonConvert.SerializeObject(node);
        var error = Assert.Throws<InvalidOperationException>(() => Create().Convert(node, new()));
        Assert.Contains("name_1", error.Message);
        Assert.Equal(before, JsonConvert.SerializeObject(node));
    }

    [Theory]
    [InlineData("{1}", "name_{0}")]
    [InlineData("constant", "name_{0}")]
    [InlineData("var_{0}_name", "count")]
    public void InvalidOrOverlappingFormatsFailWithoutMutation(string original, string target)
    {
        var config = JObject.Parse(Mapping);
        config["PropertyRules"]![0]!["OriginalKeyFormat"] = original;
        config["PropertyRules"]![0]!["NewKeyFormat"] = target;
        var node = Node("1", ("var_1_name", "a"));
        var before = JsonConvert.SerializeObject(node);
        Assert.ThrowsAny<Exception>(() => Create(config).Convert(node, new()));
        Assert.Equal(before, JsonConvert.SerializeObject(node));
    }

    [Fact]
    public void ExistingPropertyConverterCanQuoteAnItemField()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var config = JObject.Parse(Mapping);
        config["ItemConverters"] = JArray.Parse("""
            [{"$type":"NodePropertyRemapping","OriginalKey":"name","NewKey":"name","AsStringLiteral":true}]
            """);
        var converter = Deserialize(config.ToString(), services);
        var node = Node("1", ("var_1_name", "x'y"));
        converter.Convert(node, new());
        Assert.Equal("\"x\\'y\"", node.GetProperty("name_0"));
    }

    [Fact]
    public void ItemPipelineRejectsTypeRemappingAndLeavesOuterContextAlone()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var config = JObject.Parse(Mapping);
        config["ItemConverters"] = JArray.Parse("""
            [{"$type":"NodeTypeRemapping","To":"Code"}]
            """);
        var node = Node("1", ("var_1_name", "a"));
        var before = JsonConvert.SerializeObject(node);
        var context = new SharpNodeFormattingContext { ShouldRetry = true };
        Assert.Throws<InvalidOperationException>(() => Deserialize(config.ToString(), services).Convert(node, context));
        Assert.Equal(before, JsonConvert.SerializeObject(node));
        Assert.True(context.ShouldRetry);
    }

    [Fact]
    public void NodeActivityAndPhysicalSubtreesArePreserved()
    {
        var node = Node("1", ("var_1_name", "a"));
        var parent = new NodeData("Folder");
        var child = new NodeData("Code");
        parent.Add(node);
        node.Add(child);
        node.IsActive = false;
        Create().Convert(node, new());
        Assert.False(node.IsActive);
        Assert.Same(parent, node.PhysicalParent);
        Assert.Same(child, Assert.Single(node.PhysicalChildren));
        Assert.Same(node, child.PhysicalParent);
    }

    [Theory]
    [InlineData(null, "0")]
    [InlineData("", "0")]
    [InlineData(" ", " ")]
    [InlineData("2", "2")]
    public void DefaultValueOnlyFillsMissingOrEmptyStrings(string? input, string expected)
    {
        var converter = JsonConvert.DeserializeObject<PropertyDefaultValueConverter>("""
            {"Key":"delay","DefaultValue":"0"}
            """)!;
        var node = new NodeData("SmoothSetValueTo");
        if (input is not null) node.Properties["delay"] = input;
        converter.Convert(node, new());
        Assert.Equal(expected, node.GetProperty("delay"));
    }

    [Fact]
    public void ConverterShortNamesResolveAndSurviveConfigurationRoundtrip()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var settings = Settings(services);
        foreach (var json in new[]
        {
            JObject.Parse(Mapping).ToString(),
            "{\"Key\":\"delay\",\"DefaultValue\":\"0\"}"
        })
        {
            var config = JObject.Parse(json);
            config["$type"] = config.ContainsKey("Key") ? "PropertyDefaultValue" : "IndexedListRemapping";
            var converter = JsonConvert.DeserializeObject<ISharpNodeFormatConverter>(config.ToString(), settings)!;
            Assert.NotNull(converter);
            var serialized = JObject.Parse(JsonConvert.SerializeObject(converter));
            serialized["$type"] = config["$type"];
            Assert.IsType(converter.GetType(), JsonConvert.DeserializeObject<ISharpNodeFormatConverter>(serialized.ToString(), settings));
        }
    }

    [Theory]
    [InlineData("LocalVar", "LocalVariable", "init_value", "value_0")]
    [InlineData("Assignment", "Assignment", "value", "value_0")]
    [InlineData("RecordPos", "RecordPos", "object", "object_0")]
    public void ActualPackageMappingsLoadAndKeepDisabledData(string legacyType, string expectedType,
        string legacyValueSuffix, string expectedValueKey)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var path = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "configs"),
            $".Data.{legacyType}, LuaSTGEditorSharp.sharpconv", SearchOption.AllDirectories).Single();
        var converter = new SharpNodeFormatConverter(services);
        JsonConvert.PopulateObject(File.ReadAllText(path), converter, Settings(services));
        var node = new NodeData($".Data.{legacyType}, LuaSTGEditorSharp") { IsActive = false };
        node.Properties["Number of Var"] = "1";
        node.Properties["Var 1 name"] = "a";
        node.Properties[$"Var 1 {legacyValueSuffix.Replace('_', ' ')}"] = "self";
        converter.Convert(node, new());
        Assert.Equal(expectedType, node.TypeUID);
        Assert.Equal("a", node.GetProperty("name_0"));
        Assert.Equal("self", node.GetProperty(expectedValueKey));
        Assert.False(node.IsActive);
        var restored = JsonConvert.DeserializeObject<NodeData>(JsonConvert.SerializeObject(node))!;
        Assert.Equal(node.Properties, restored.Properties);
    }

    [Fact]
    public void RepeatPackageCompositionMatchesHistoricalConverter()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var converter = Load(".General.Repeat, LuaSTGEditorSharp.sharpconv", services);
        var node = new NodeData(".General.Repeat, LuaSTGEditorSharp");
        node.Properties["Number of Var"] = "2";
        node.Properties["Var 1 name"] = "a";
        node.Properties["Var 1 init value"] = "0";
        node.Properties["Var 1 increment"] = "1/t";
        node.Properties["Var 2 name"] = "";
        node.Properties["Var 2 init value"] = "";
        node.Properties["Var 2 increment"] = "";
        var expected = node.DeepClone();
        new DefaultSharpNodeFormatConverter().Convert(expected, new());
        new RepeatVariableConverter().Convert(expected, new());
        converter.Convert(node, new());
        Assert.Equal(expected.Properties, node.Properties);
    }

    [Fact]
    public void SmoothPackageCompositionMatchesHistoricalConverterIncludingInactiveFields()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var converter = Load(".Task.SmoothSetValueTo, .sharpconv", services);
        var node = new NodeData(".Task.SmoothSetValueTo, ");
        node.Properties["Number of Value"] = "1";
        node.Properties["Waiting time"] = "";
        node.Properties["Duration"] = "";
        node.Properties["Value 1 name"] = "alpha";
        node.Properties["Value 2 name"] = "x'y";
        node.Properties["Target value 2"] = "15";
        var expected = node.DeepClone();
        new DefaultSharpNodeFormatConverter().Convert(expected, new());
        new SmoothSetValueToConverter().Convert(expected, new());
        converter.Convert(node, new());
        Assert.Equal(expected.Properties.OrderBy(pair => pair.Key), node.Properties.OrderBy(pair => pair.Key));
    }

    private static SharpNodeFormatConverter Load(string file, IServiceProvider services)
    {
        var path = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "configs"), file,
            SearchOption.AllDirectories).Single();
        var converter = new SharpNodeFormatConverter(services);
        JsonConvert.PopulateObject(File.ReadAllText(path), converter, Settings(services));
        return converter;
    }

    private static IndexedListRemappingConverter Create(JObject? config = null)
        => JsonConvert.DeserializeObject<IndexedListRemappingConverter>((config ?? JObject.Parse(Mapping)).ToString())!;

    private static IndexedListRemappingConverter Deserialize(string config, IServiceProvider services)
        => JsonConvert.DeserializeObject<IndexedListRemappingConverter>(config, Settings(services))!;

    private static JsonSerializerSettings Settings(IServiceProvider services)
        => new() { Converters = { new SerializationTypeBindingService(services) } };

    private static NodeData Node(string count, params (string Key, string Value)[] values)
    {
        var node = new NodeData("List");
        node.Properties["old_count"] = count;
        foreach (var value in values) node.Properties.Add(value.Key, value.Value);
        return node;
    }
}
