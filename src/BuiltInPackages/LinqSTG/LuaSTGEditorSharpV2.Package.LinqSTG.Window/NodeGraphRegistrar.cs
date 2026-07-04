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
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator;
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

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGPortView(), typeof(IViewFor<LinqSTGPortViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGConnectionView(), typeof(IViewFor<LinqSTGConnectionViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<ContextAwareNodeInputViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<ContextAwareNodeOutputViewModel>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ConstantFloatNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ConstantIntNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<ConstantStringNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<RepeaterKeyNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Vector2FromRotationDistanceNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Vector2Node>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<AddNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<FloatToIntNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<IntToFloatNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MinMaxNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Sample01Node>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<Sample01MinMaxNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TakeRepeaterFromContextNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<TakeVariableFromContextNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<StationaryMovementNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<UniformVelocityMovementNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<UniformAccelerationMovementNode>));

            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementSumNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementOffsetNode>));
            Splat.Locator.CurrentMutable.Register(() => new LinqSTGNodeView(), typeof(IViewFor<MovementAfterTimeNode>));

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
