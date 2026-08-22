using global::LinqSTG.Kinematics;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementTransformOperator;
using DynamicData;
using NodeNetwork.ViewModels;
using System.Numerics;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Unified custom movement-transform path (Movement-Predict-FromPointMovement)
    /// semantics on the live node view models, driven through real port connections.
    /// FromPointMovement evaluates its point subgraph lazily per sample time t,
    /// injecting TransformContext{Time=t}; InputTime reads t, Predict samples a
    /// wired movement at any t'.
    /// </summary>
    public class MovementTransformSemanticsTests
    {
        private static T Latest<T>(IObservable<T>? source)
        {
            Assert.NotNull(source);
            T latest = default!;
            using var subscription = source.Subscribe(v => latest = v);
            return latest;
        }

        private static Vector2 PredictAt(Contextual<IParametric<int, Vector2>> movement, Parameter parameter, int t)
            => movement(parameter).Predict(t);

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

        private static T NodeAt<T>(NetworkViewModel network, int index)
            where T : LinqSTGNodeViewModel
            => (T)network.Nodes.Items.ElementAt(index);

        [Fact]
        public void InputTime_ReadsSampleScopeTime()
        {
            var node = new MovementTransformInputTimeNode();
            var time = Latest(node.OutputTime.Value);

            Assert.Equal(7, time(new Parameter().WithTransform(new TransformContext { Time = 7 })));
            Assert.Equal(0, time(Parameter.Empty));
        }

        [Fact]
        public void Predict_Degradations_WhenPortsUnconnected()
        {
            var node = new MovementPredictNode();
            var point = Latest(node.OutputPoint.Value);

            // Both ports unconnected: zero movement sampled at 0.
            Assert.Equal(new Vector2(0, 0), point(Parameter.Empty));
        }

        [Fact]
        public void Predict_SamplesWiredMovementAtWiredTime()
        {
            var network = Network(new Vector2Node(), new UniformVelocityMovementNode(), new MovementPredictNode());
            var vec = NodeAt<Vector2Node>(network, 0);
            var source = NodeAt<UniformVelocityMovementNode>(network, 1);
            var predict = NodeAt<MovementPredictNode>(network, 2);
            vec.XEditor.RawValue = 3f;
            vec.YEditor.RawValue = 5f;
            Connect(network, source.InputVelocity, vec.OutputVector2);
            Connect(network, predict.InputMovement, source.OutputMovement);

            var point = Latest(predict.OutputPoint.Value);

            // Time unconnected -> samples at t = 0.
            Assert.Equal(new Vector2(0, 0), point(Parameter.Empty));
        }

        [Fact]
        public void FromPointMovement_WrapsTimeIndependentPointIntoConstantMovement()
        {
            var network = Network(new Vector2Node(), new FromPointMovementNode());
            var vec = NodeAt<Vector2Node>(network, 0);
            var fromPoint = NodeAt<FromPointMovementNode>(network, 1);
            vec.XEditor.RawValue = 3f;
            vec.YEditor.RawValue = 4f;
            Connect(network, fromPoint.InputPosition, vec.OutputVector2);

            var movement = Latest(fromPoint.OutputMovement.Value);

            Assert.Equal(new Vector2(3, 4), PredictAt(movement, Parameter.Empty, 0));
            Assert.Equal(new Vector2(3, 4), PredictAt(movement, Parameter.Empty, 100));
        }

        [Fact]
        public void UnifiedIdentityPath_ReproducesMovement()
        {
            // FromPointMovement(Predict(UniformVelocity(1,2), InputTime)) == identity.
            var network = Network(
                new Vector2Node(),
                new UniformVelocityMovementNode(),
                new MovementTransformInputTimeNode(),
                new MovementPredictNode(),
                new FromPointMovementNode());
            var vec = NodeAt<Vector2Node>(network, 0);
            var source = NodeAt<UniformVelocityMovementNode>(network, 1);
            var inputTime = NodeAt<MovementTransformInputTimeNode>(network, 2);
            var predict = NodeAt<MovementPredictNode>(network, 3);
            var fromPoint = NodeAt<FromPointMovementNode>(network, 4);
            vec.XEditor.RawValue = 1f;
            vec.YEditor.RawValue = 2f;
            Connect(network, source.InputVelocity, vec.OutputVector2);
            Connect(network, predict.InputMovement, source.OutputMovement);
            Connect(network, predict.InputTime, inputTime.OutputTime);
            Connect(network, fromPoint.InputPosition, predict.OutputPoint);

            var movement = Latest(fromPoint.OutputMovement.Value);

            // The point subgraph is re-evaluated per sample time t, so the custom
            // transform yields back the source movement m(t) = (t, 2t).
            Assert.Equal(new Vector2(0, 0), PredictAt(movement, Parameter.Empty, 0));
            Assert.Equal(new Vector2(10, 20), PredictAt(movement, Parameter.Empty, 10));
        }

        [Fact]
        public void UnifiedTimeShiftPath_SamplesMovementAtShiftedTime()
        {
            // FromPointMovement(Predict(UniformVelocity(3,5), InputTime + 1)) == f(t) => f(t+1).
            var network = Network(
                new Vector2Node(),
                new UniformVelocityMovementNode(),
                new MovementTransformInputTimeNode(),
                new ConstantIntNode(),
                new AddNode(),
                new MovementPredictNode(),
                new FromPointMovementNode());
            var vec = NodeAt<Vector2Node>(network, 0);
            var source = NodeAt<UniformVelocityMovementNode>(network, 1);
            var inputTime = NodeAt<MovementTransformInputTimeNode>(network, 2);
            var one = NodeAt<ConstantIntNode>(network, 3);
            var add = NodeAt<AddNode>(network, 4);
            var predict = NodeAt<MovementPredictNode>(network, 5);
            var fromPoint = NodeAt<FromPointMovementNode>(network, 6);
            vec.XEditor.RawValue = 3f;
            vec.YEditor.RawValue = 5f;
            one.ValueEditor.RawValue = 1;
            Connect(network, source.InputVelocity, vec.OutputVector2);
            Connect(network, add.NumericA, inputTime.OutputTime);
            Connect(network, add.NumericB, one.OutputValue);
            Connect(network, predict.InputMovement, source.OutputMovement);
            Connect(network, predict.InputTime, add.NumericResult);
            Connect(network, fromPoint.InputPosition, predict.OutputPoint);

            var movement = Latest(fromPoint.OutputMovement.Value);

            // m(t) = (3t, 5t); custom transform f(t) => f(t+1).
            Assert.Equal(new Vector2(3, 5), PredictAt(movement, Parameter.Empty, 0));
            Assert.Equal(new Vector2(30, 50), PredictAt(movement, Parameter.Empty, 9));
        }

        [Fact]
        public void UnifiedPath_ComposesWithBuiltInOperators()
        {
            // The custom transform is itself a movement, so built-in operators
            // compose on top of it: Offset(f(t)=>f(t+1), (10, 20)).
            var network = Network(
                new Vector2Node(),
                new UniformVelocityMovementNode(),
                new MovementTransformInputTimeNode(),
                new ConstantIntNode(),
                new AddNode(),
                new MovementPredictNode(),
                new FromPointMovementNode(),
                new Vector2Node(),
                new MovementOffsetNode());
            var vec = NodeAt<Vector2Node>(network, 0);
            var source = NodeAt<UniformVelocityMovementNode>(network, 1);
            var inputTime = NodeAt<MovementTransformInputTimeNode>(network, 2);
            var one = NodeAt<ConstantIntNode>(network, 3);
            var add = NodeAt<AddNode>(network, 4);
            var predict = NodeAt<MovementPredictNode>(network, 5);
            var fromPoint = NodeAt<FromPointMovementNode>(network, 6);
            var offsetVec = NodeAt<Vector2Node>(network, 7);
            var offset = NodeAt<MovementOffsetNode>(network, 8);
            vec.XEditor.RawValue = 3f;
            vec.YEditor.RawValue = 5f;
            one.ValueEditor.RawValue = 1;
            offsetVec.XEditor.RawValue = 10f;
            offsetVec.YEditor.RawValue = 20f;
            Connect(network, source.InputVelocity, vec.OutputVector2);
            Connect(network, add.NumericA, inputTime.OutputTime);
            Connect(network, add.NumericB, one.OutputValue);
            Connect(network, predict.InputMovement, source.OutputMovement);
            Connect(network, predict.InputTime, add.NumericResult);
            Connect(network, fromPoint.InputPosition, predict.OutputPoint);
            Connect(network, offset.InputMovement, fromPoint.OutputMovement);
            Connect(network, offset.InputOffset, offsetVec.OutputVector2);

            var movement = Latest(offset.OutputMovement.Value);

            // m(t+1) + (10, 20) = (3t+13, 5t+25).
            Assert.Equal(new Vector2(13, 25), PredictAt(movement, Parameter.Empty, 0));
            Assert.Equal(new Vector2(43, 75), PredictAt(movement, Parameter.Empty, 10));
        }

        [Fact]
        public void ScaleTime_ScalesSamplingTime()
        {
            var network = Network(new Vector2Node(), new UniformVelocityMovementNode(), new MovementScaleTimeNode());
            var vec = NodeAt<Vector2Node>(network, 0);
            var source = NodeAt<UniformVelocityMovementNode>(network, 1);
            var scaleTime = NodeAt<MovementScaleTimeNode>(network, 2);
            vec.XEditor.RawValue = 1f;
            vec.YEditor.RawValue = 2f;
            scaleTime.InputFactorEditor.RawValue = 2f;
            Connect(network, source.InputVelocity, vec.OutputVector2);
            Connect(network, scaleTime.InputMovement, source.OutputMovement);

            var movement = Latest(scaleTime.OutputMovement.Value);

            // m(t) = (t, 2t); scale time by 2 -> result(t) = m(2t) = (2t, 4t).
            Assert.Equal(new Vector2(0, 0), PredictAt(movement, Parameter.Empty, 0));
            Assert.Equal(new Vector2(8, 16), PredictAt(movement, Parameter.Empty, 4));
        }

        [Fact]
        public void ShiftTime_ShiftsSamplingTime()
        {
            var network = Network(new Vector2Node(), new UniformVelocityMovementNode(), new MovementShiftTimeNode());
            var vec = NodeAt<Vector2Node>(network, 0);
            var source = NodeAt<UniformVelocityMovementNode>(network, 1);
            var shiftTime = NodeAt<MovementShiftTimeNode>(network, 2);
            vec.XEditor.RawValue = 1f;
            vec.YEditor.RawValue = 2f;
            shiftTime.InputDeltaEditor.RawValue = 3;
            Connect(network, source.InputVelocity, vec.OutputVector2);
            Connect(network, shiftTime.InputMovement, source.OutputMovement);

            var movement = Latest(shiftTime.OutputMovement.Value);

            // m(t) = (t, 2t); shift time by 3 -> result(t) = m(t+3) = (t+3, 2t+6).
            Assert.Equal(new Vector2(3, 6), PredictAt(movement, Parameter.Empty, 0));
            Assert.Equal(new Vector2(6, 12), PredictAt(movement, Parameter.Empty, 3));
        }
    }
}
