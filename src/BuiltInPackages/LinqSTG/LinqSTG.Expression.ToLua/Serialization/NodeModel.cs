using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LinqSTG.Expression.ToLua.Serialization
{
    public record class NodeModel
    (
        [property: JsonProperty("type", Required = Required.Always)] string NodeType,
        [property: JsonProperty("x")] double X,
        [property: JsonProperty("y")] double Y,
        [property: JsonProperty("editors", Required = Required.Always)] JObject Editors
    );
}
