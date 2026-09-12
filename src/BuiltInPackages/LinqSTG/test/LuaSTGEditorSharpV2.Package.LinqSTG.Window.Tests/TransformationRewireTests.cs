using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Transformation;
using NodeNetwork.ViewModels;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Reproduction scenarios for transformation-chain rewiring:
    /// connect A→B, disconnect, reconnect as B→A (the reported crash sequence),
    /// and variants that leave a cycle in the connection graph.
    /// </summary>
    public class TransformationRewireTests
    {
        private static T Latest<T>(IObservable<T>? source)
        {
            Assert.NotNull(source);
            T latest = default!;
            using var subscription = source!.Subscribe(v => latest = v);
            return latest;
        }

        private static NetworkViewModel Network(params LinqSTGNodeViewModel[] nodes)
        {
            var network = new NetworkViewModel();
            foreach (var node in nodes)
            {
                network.Nodes.Add(node);
            }
            return network;
        }

        private static LinqSTGConnectionViewModel Connect(NetworkViewModel network, NodeInputViewModel input, NodeOutputViewModel output)
        {
            var connection = new LinqSTGConnectionViewModel(network, input, output);
            network.Connections.Add(connection);
            return connection;
        }

        [Fact]
        public void CleanRewire_DisconnectThenReverse_DoesNotOverflow()
        {
            var network = Network(new Vector2AssignmentNode(), new Vector2AssignmentNode());
            var a = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(0);
            var b = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(1);
            a.KeyXEditor.RawValue = "ax";
            a.KeyYEditor.RawValue = "ay";
            b.KeyXEditor.RawValue = "bx";
            b.KeyYEditor.RawValue = "by";

            // A -> B, then break it, then rewire as B -> A.
            var ab = Connect(network, b.InputTransformation, a.OutputTransformation);
            network.Connections.Remove(ab);
            Connect(network, a.InputTransformation, b.OutputTransformation);

            // Evaluating A runs B's assignment first, then A's own.
            var transformation = Latest(a.OutputTransformation.Value);
            var result = transformation(new Parameter());

            Assert.Equal(0f, result.Floats["bx"]);
            Assert.Equal(0f, result.Floats["by"]);
            Assert.Equal(0f, result.Floats["ax"]);
            Assert.Equal(0f, result.Floats["ay"]);
        }

        [Fact]
        public void CycleDetector_AllowsReverse_AfterForwardRemoved()
        {
            var network = Network(new Vector2AssignmentNode(), new Vector2AssignmentNode());
            var a = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(0);
            var b = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(1);

            // While A -> B exists, wiring B -> A would close a directed cycle.
            var ab = Connect(network, b.InputTransformation, a.OutputTransformation);
            Assert.True(ConnectionCycleDetector.WouldCreateCycle(a.InputTransformation, b.OutputTransformation));

            // After the removal the reverse direction is a plain chain again.
            network.Connections.Remove(ab);
            Assert.False(ConnectionCycleDetector.WouldCreateCycle(a.InputTransformation, b.OutputTransformation));
        }

        [Fact]
        public void CycleDetector_DetectsLongerCycles()
        {
            var network = Network(new Vector2AssignmentNode(), new Vector2AssignmentNode(), new Vector2AssignmentNode());
            var a = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(0);
            var b = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(1);
            var c = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(2);

            Connect(network, b.InputTransformation, a.OutputTransformation);
            Connect(network, c.InputTransformation, b.OutputTransformation);

            // C -> A would close the A -> B -> C -> A cycle; a shortcut A -> C stays acyclic.
            Assert.True(ConnectionCycleDetector.WouldCreateCycle(a.InputTransformation, c.OutputTransformation));
            Assert.False(ConnectionCycleDetector.WouldCreateCycle(c.InputTransformation, a.OutputTransformation));
        }

        [Fact]
        public void ConnectionValidator_RejectsReverseConnection_WhileForwardExists()
        {
            var network = new NetworkViewModel();
            var a = new Vector2AssignmentNode();
            var b = new Vector2AssignmentNode();
            network.Nodes.Add(a);
            network.Nodes.Add(b);
            network.Connections.Add(new LinqSTGConnectionViewModel(network, b.InputTransformation, a.OutputTransformation));

            var pending = new NodeNetwork.ViewModels.PendingConnectionViewModel(network)
            {
                Output = b.OutputTransformation,
            };

            var validation = a.InputTransformation.ConnectionValidator!(pending);

            Assert.False(validation.IsValid);
        }

        [Fact]
        public void ConnectionValidator_AllowsReverseConnection_AfterForwardRemoved()
        {
            var network = new NetworkViewModel();
            var a = new Vector2AssignmentNode();
            var b = new Vector2AssignmentNode();
            network.Nodes.Add(a);
            network.Nodes.Add(b);

            var pending = new NodeNetwork.ViewModels.PendingConnectionViewModel(network)
            {
                Output = b.OutputTransformation,
            };

            var validation = a.InputTransformation.ConnectionValidator!(pending);

            Assert.True(validation.IsValid);
        }
    }
}
