using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View.Editor;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementTransformOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Transformation;

using NodeNetwork;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Views;

using ReactiveUI;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public static class NodeGraphRegistrar
    {
        private static int _registered;
        private static readonly string _packageDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        public static void Register()
        {
            if (Interlocked.Exchange(ref _registered, 1) != 0) return;

            AppDomain.CurrentDomain.AssemblyResolve += HandlePackageAssemblyResolve;

            NNViewRegistrar.RegisterSplat();

            Splat.Locator.CurrentMutable.Register(() => new IntegerValueEditorView(), typeof(IViewFor<IntegerValueEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new StringValueEditorView(), typeof(IViewFor<StringValueEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new FloatValueEditorView(), typeof(IViewFor<FloatValueEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new IntervalTypeEditorView(), typeof(IViewFor<IntervalTypeEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new BulletShapeEditorView(), typeof(IViewFor<BulletShapeEditorViewModel>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGPortView(), typeof(IViewFor<LinqSTGPortViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGConnectionView(), typeof(IViewFor<LinqSTGConnectionViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<ContextAwareNodeInputViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<ContextAwareNodeOutputViewModel>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ConstantFloatNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ConstantIntNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ConstantStringNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RepeaterKeyNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<PatternVariableFloatNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<PatternVariableIntNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<InfiniteNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SelfPositionNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<PlayerPositionNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Vector2FromRotationDistanceNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Vector2Node>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<AddNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SubtractNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MultiplyNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<DivideNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ModuloNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<NegateNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SinNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<CosNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TanNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ASinNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ACosNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ATanNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<DegToRadNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RadToDegNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<AbsNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SqrtNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<FloorNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<CeilNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SignNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ExpNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<LogNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ATan2Node>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<PowNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MinNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MaxNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ClampNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<LerpNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RandomFloatNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RandomIntNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RandomSignNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<FloatToIntNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<IntToFloatNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MinMaxNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Sample01Node>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Sample01MinMaxNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TakeRepeaterFromContextNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TakeVariableFromContextNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<FromPointMovementNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<UniformVelocityMovementNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<UniformAccelerationMovementNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementSumNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementOffsetNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementAfterTimeNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementRotateNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementScaleNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementCartesianToPolarNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementPolarToCartesianNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementScaleTimeNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementShiftTimeNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementTransformInputTimeNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementPredictNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Vector2SplitNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RotateVectorNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RepeatPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RepeatWithIntervalPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SingleDataPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SingleIntervalPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<EmptyPatternNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MapPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ExtrudePatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ExtrudeConcatPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<FilterPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ConcatPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ReversePatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SkipPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TakePatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<SkipWhilePatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TakeWhilePatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TrimStartPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TrimEndPatternNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TrimPatternNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<AssignNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ShootNode>));
        }

        private static Assembly? HandlePackageAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            var simpleName = new AssemblyName(args.Name).Name;
            if (string.IsNullOrEmpty(simpleName) || string.IsNullOrEmpty(_packageDirectory))
            {
                return null;
            }

            var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == simpleName);
            if (alreadyLoaded != null)
            {
                return alreadyLoaded;
            }

            var candidate = Path.Combine(_packageDirectory, simpleName + ".dll");
            return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
        }
    }
}
