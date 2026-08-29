using Newtonsoft.Json;

namespace LinqSTG.Expression.ToLua.Serialization
{
    public record class NetworkModel
    (
        NodeModel[] Nodes,
        ConnectionModel[] Connections,
        [property: JsonProperty("variables", NullValueHandling = NullValueHandling.Ignore)]
        VariableItemModel[]? Variables = null,
        [property: JsonProperty("seed")]
        int Seed = 0
    );
}
