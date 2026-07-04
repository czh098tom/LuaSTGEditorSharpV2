namespace LinqSTG.Expression.ToLua.Serialization
{
    public record class NetworkModel
    (
        NodeModel[] Nodes,
        ConnectionModel[] Connections
    );
}
