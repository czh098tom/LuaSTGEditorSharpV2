using Newtonsoft.Json;

namespace LinqSTG.Expression.ToLua.Serialization
{
    public record class ConnectionModel
    (
        [property: JsonProperty("source_node_index", Required = Required.Always)] int SourceNodeIndex,
        [property: JsonProperty("source_port_name", Required = Required.Always)] string SourcePortName,
        [property: JsonProperty("target_node_index", Required = Required.Always)] int TargetNodeIndex,
        [property: JsonProperty("target_port_name", Required = Required.Always)] string TargetPortName
    );
}
