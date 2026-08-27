using Newtonsoft.Json;

namespace LinqSTG.Expression.ToLua.Serialization
{
    /// <summary>
    /// One entry of the blueprint window's variable list: a name bound to a
    /// numeric value, where <see cref="IsInteger"/> records whether the value
    /// was authored as an integer or a float.
    /// </summary>
    public record class VariableItemModel
    (
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("value")] double Value,
        [property: JsonProperty("isInt")] bool IsInteger
    );
}
