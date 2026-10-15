using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache;

/// <summary>Renders each accepted row and joins its fragments without implicit newlines.</summary>
public sealed class JoinedListColumn
{
    [JsonProperty] public string[] Template { get; private set; } = [];
    [JsonProperty] public string Separator { get; private set; } = string.Empty;
}
