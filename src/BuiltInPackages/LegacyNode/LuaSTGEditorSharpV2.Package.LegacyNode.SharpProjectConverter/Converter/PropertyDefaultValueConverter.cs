using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

[JsonTypeShortName(typeof(ISharpNodeFormatConverter), "PropertyDefaultValue")]
public sealed class PropertyDefaultValueConverter : ISharpNodeFormatConverter
{
    [JsonProperty(Required = Required.Always)] public string Key { get; private set; } = string.Empty;
    [JsonProperty(Required = Required.Always)] public string DefaultValue { get; private set; } = string.Empty;

    public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
    {
        if (string.IsNullOrEmpty(Key))
        {
            throw new InvalidOperationException("PropertyDefaultValue.Key must not be empty.");
        }
        if (!source.Properties.TryGetValue(Key, out var value) || string.IsNullOrEmpty(value))
        {
            source.Properties[Key] = DefaultValue;
        }
        return source;
    }
}
