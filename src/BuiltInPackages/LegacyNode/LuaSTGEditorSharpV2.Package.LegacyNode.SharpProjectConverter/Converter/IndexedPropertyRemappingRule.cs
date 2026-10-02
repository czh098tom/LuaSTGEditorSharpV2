using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>Maps one property column through an isolated item property dictionary.</summary>
public sealed class IndexedPropertyRemappingRule
{
    [JsonProperty(Required = Required.Always)] public string OriginalKeyFormat { get; private set; } = string.Empty;
    [JsonProperty(Required = Required.Always)] public string ItemKey { get; private set; } = string.Empty;
    [JsonProperty(Required = Required.Always)] public string NewKeyFormat { get; private set; } = string.Empty;
    [JsonProperty] public string? DefaultValue { get; private set; }
}
