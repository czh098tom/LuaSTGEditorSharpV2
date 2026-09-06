using Newtonsoft.Json;

namespace LinqSTG.Expression.ToLua.Serialization
{
    /// <summary>
    /// One entry of the blueprint window's variable list: a name bound to a
    /// numeric value, where <see cref="IsInteger"/> records whether the value
    /// was authored as an integer or a float. A non-null <see cref="ValueY"/>
    /// marks a vector2 entry (locked-only in the UI: self/player positions),
    /// with <see cref="Value"/> holding the X component.
    /// </summary>
    public record class VariableItemModel
    (
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("value")] double Value,
        [property: JsonProperty("isInt")] bool IsInteger,
        [property: JsonProperty("valueY", NullValueHandling = NullValueHandling.Ignore)]
        double? ValueY = null
    );
}
