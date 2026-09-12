using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Transformation;
using NodeNetwork.ViewModels;
using System.Numerics;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Preview semantics of the aggregated vector2 variable nodes:
    /// 二维变量 bundles two Variable nodes over the Floats scope with a direct
    /// vector2 output port, and 二维向量赋值 writes a vector input's components
    /// back into the same scope under the two editor-backed names (defaults x/y).
    /// </summary>
    public class Vector2VariableNodeTests
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

        private static void Connect(NetworkViewModel network, NodeInputViewModel input, NodeOutputViewModel output)
            => network.Connections.Add(new LinqSTGConnectionViewModel(network, input, output));

        [Fact]
        public void KeyEditors_DefaultToXAndY()
        {
            var variable = new Vector2VariableNode();
            var assignment = new Vector2AssignmentNode();

            Assert.Equal("x", variable.KeyXEditor.RawValue);
            Assert.Equal("y", variable.KeyYEditor.RawValue);
            Assert.Equal("x", assignment.KeyXEditor.RawValue);
            Assert.Equal("y", assignment.KeyYEditor.RawValue);
        }

        [Fact]
        public void Vector2Variable_KeyOutputs_PassEditorNamesThrough()
        {
            var node = new Vector2VariableNode();
            node.KeyXEditor.RawValue = "px";
            node.KeyYEditor.RawValue = "py";

            var keyX = Latest(node.OutputKeyX.Value);
            var keyY = Latest(node.OutputKeyY.Value);

            Assert.Equal("px", keyX(new Parameter()));
            Assert.Equal("py", keyY(new Parameter()));
        }

        [Fact]
        public void Vector2Variable_ReadsBothFloatEntriesIntoVector2()
        {
            var node = new Vector2VariableNode();
            var parameter = new Parameter();
            parameter.Floats["x"] = 3f;
            parameter.Floats["y"] = 4f;

            var vector = Latest(node.OutputVector2.Value);
            var x = Latest(node.OutputX.Value);
            var y = Latest(node.OutputY.Value);

            Assert.Equal(new Vector2(3f, 4f), vector(parameter));
            Assert.Equal(3f, x(parameter));
            Assert.Equal(4f, y(parameter));
        }

        [Fact]
        public void Vector2Variable_MissingEntriesFallBackToZero()
        {
            var node = new Vector2VariableNode();

            var vector = Latest(node.OutputVector2.Value);
            var x = Latest(node.OutputX.Value);
            var y = Latest(node.OutputY.Value);

            Assert.Equal(Vector2.Zero, vector(new Parameter()));
            Assert.Equal(0f, x(new Parameter()));
            Assert.Equal(0f, y(new Parameter()));
        }

        [Fact]
        public void Vector2Variable_CustomKeys_ResolveFromEditors()
        {
            var node = new Vector2VariableNode();
            node.KeyXEditor.RawValue = "px";
            node.KeyYEditor.RawValue = "py";
            var parameter = new Parameter();
            parameter.Floats["px"] = 1f;
            parameter.Floats["py"] = 2f;

            var vector = Latest(node.OutputVector2.Value);

            Assert.Equal(new Vector2(1f, 2f), vector(parameter));
        }

        [Fact]
        public void Vector2Assignment_WritesBothComponentsToFloatScope()
        {
            var network = Network(new Vector2Node(), new Vector2AssignmentNode());
            var vec = (Vector2Node)network.Nodes.Items.ElementAt(0);
            var assign = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(1);
            Connect(network, assign.InputValue, vec.OutputVector2);
            vec.XEditor.RawValue = 3f;
            vec.YEditor.RawValue = 4f;

            var transformation = Latest(assign.OutputTransformation.Value);
            var result = transformation(new Parameter());

            Assert.Equal(3f, result.Floats["x"]);
            Assert.Equal(4f, result.Floats["y"]);
        }

        [Fact]
        public void Vector2Assignment_CustomKeys_WriteToNamedEntries()
        {
            var network = Network(new Vector2Node(), new Vector2AssignmentNode());
            var vec = (Vector2Node)network.Nodes.Items.ElementAt(0);
            var assign = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(1);
            Connect(network, assign.InputValue, vec.OutputVector2);
            vec.XEditor.RawValue = 1f;
            vec.YEditor.RawValue = 2f;
            assign.KeyXEditor.RawValue = "px";
            assign.KeyYEditor.RawValue = "py";

            var transformation = Latest(assign.OutputTransformation.Value);
            var result = transformation(new Parameter());

            Assert.Equal(1f, result.Floats["px"]);
            Assert.Equal(2f, result.Floats["py"]);
        }

        [Fact]
        public void Vector2Assignment_UnconnectedValueAssignsZeroVector()
        {
            var assign = new Vector2AssignmentNode();

            var transformation = Latest(assign.OutputTransformation.Value);
            var result = transformation(new Parameter());

            Assert.Equal(0f, result.Floats["x"]);
            Assert.Equal(0f, result.Floats["y"]);
        }

        [Fact]
        public void Vector2Assignment_ChainsPreviousTransformation()
        {
            var network = Network(new ConstantFloatNode(), new ConstantStringNode(), new Vector2Node(), new AssignNode(), new Vector2AssignmentNode());
            var floatValue = (ConstantFloatNode)network.Nodes.Items.ElementAt(0);
            var stringKey = (ConstantStringNode)network.Nodes.Items.ElementAt(1);
            var vec = (Vector2Node)network.Nodes.Items.ElementAt(2);
            var prev = (AssignNode)network.Nodes.Items.ElementAt(3);
            var assign = (Vector2AssignmentNode)network.Nodes.Items.ElementAt(4);
            Connect(network, prev.InputValue, floatValue.OutputValue);
            Connect(network, prev.InputKey, stringKey.OutputValue);
            Connect(network, assign.InputValue, vec.OutputVector2);
            Connect(network, assign.InputTransformation, prev.OutputTransformation);
            floatValue.ValueEditor.RawValue = 7f;
            stringKey.ValueEditor.RawValue = "speed";
            vec.XEditor.RawValue = 3f;
            vec.YEditor.RawValue = 4f;

            var transformation = Latest(assign.OutputTransformation.Value);
            var result = transformation(new Parameter());

            // The chained scalar assignment ran first, then both components were added.
            Assert.Equal(7f, result.Floats["speed"]);
            Assert.Equal(3f, result.Floats["x"]);
            Assert.Equal(4f, result.Floats["y"]);
        }

        [Fact]
        public void Vector2Variable_RoundTripsEditorKeys()
        {
            var viewModel = new MainViewModel();
            var node = new Vector2VariableNode();
            node.KeyXEditor.RawValue = "px";
            node.KeyYEditor.RawValue = "py";
            node.Position = new System.Windows.Point(5, 5);
            viewModel.Network.Nodes.Add(node);

            viewModel.Save();
            Assert.NotNull(viewModel.NetworkJson);

            var reopened = new MainViewModel { NetworkJson = viewModel.NetworkJson };
            reopened.Load();
            var restored = reopened.Network.Nodes.Items.OfType<Vector2VariableNode>().SingleOrDefault();
            Assert.NotNull(restored);
            Assert.Equal("px", restored.KeyXEditor.RawValue);
            Assert.Equal("py", restored.KeyYEditor.RawValue);
        }

        [Fact]
        public void Vector2Assignment_RoundTripsEditorKeys()
        {
            var viewModel = new MainViewModel();
            var node = new Vector2AssignmentNode();
            node.KeyXEditor.RawValue = "px";
            node.KeyYEditor.RawValue = "py";
            node.Position = new System.Windows.Point(5, 5);
            viewModel.Network.Nodes.Add(node);

            viewModel.Save();
            Assert.NotNull(viewModel.NetworkJson);

            var reopened = new MainViewModel { NetworkJson = viewModel.NetworkJson };
            reopened.Load();
            var restored = reopened.Network.Nodes.Items.OfType<Vector2AssignmentNode>().SingleOrDefault();
            Assert.NotNull(restored);
            Assert.Equal("px", restored.KeyXEditor.RawValue);
            Assert.Equal("py", restored.KeyYEditor.RawValue);
        }
    }
}
