using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.Parsing;

public record ParserOptions
{
    public bool SpaceAfterComma { get; init; } = false;
    public bool SpaceAroundOperator { get; init; } = false;

    public static ParserOptions Default { get; set; } = new();
}

public class ParserServiceSettings
{
    [JsonProperty("space_after_comma")]
    public bool SpaceAfterComma { get; set; } = false;

    [JsonProperty("space_around_operator")]
    public bool SpaceAroundOperator { get; set; } = false;

    public ParserOptions ToParserOptions() => new()
    {
        SpaceAfterComma = SpaceAfterComma,
        SpaceAroundOperator = SpaceAroundOperator
    };
}
