using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Serialization
{
    public record class NetworkModel
        (
            NodeModel[] Nodes,
            ConnectionModel[] Connections
        )
    {
        public static NetworkModel FromViewModel(NetworkViewModel network)
        {
            if (network is null)
            {
                throw new ArgumentNullException(nameof(network), "NetworkViewModel cannot be null.");
            }
            var nodeList = network.Nodes.Items.OfType<LinqSTGNodeViewModel>().ToList();
            var connectionList = network.Connections.Items.ToList();

            var nodes = new NodeModel[network.Nodes.Count];
            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                nodes[i] = NodeModel.FromViewModel(node);
            }
            var connections = new ConnectionModel[network.Connections.Count];
            for (int i = 0; i < connectionList.Count; i++)
            {
                var connection = connectionList[i];
                connections[i] = ConnectionModel.FromViewModel(connection, nodeList);
            }
            return new(nodes, connections);
        }

        public void ApplyToNetwork(NetworkViewModel network)
        {
            if (network is null)
            {
                throw new ArgumentNullException(nameof(network), "NetworkViewModel cannot be null.");
            }
            var nodes = new LinqSTGNodeViewModel[Nodes.Length];
            network.Connections.Clear();
            network.Nodes.Clear();
            for (int i = 0; i < Nodes.Length; i++)
            {
                var vm = Nodes[i].CreateViewModel();
                network.Nodes.Add(vm);
                nodes[i] = vm;
            }

            for (int i = 0; i < Connections.Length; i++)
            {
                var connectionModel = Connections[i] 
                    ?? throw new InvalidOperationException($"ConnectionModel at index {i} is null.");

                var sourceNode = nodes[connectionModel.SourceNodeIndex];
                var targetNode = nodes[connectionModel.TargetNodeIndex];
                if (sourceNode is null || targetNode is null)
                {
                    throw new InvalidOperationException($"Source or target node not found for connection at index {i}.");
                }
                var sourcePort = sourceNode.OutputDict[connectionModel.SourcePortName];
                var targetPort = targetNode.InputDict[connectionModel.TargetPortName];
                network.Connections.Add(new LinqSTGConnectionViewModel(network, targetPort, sourcePort));
            }
        }
    }
}
