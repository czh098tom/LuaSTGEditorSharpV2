using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using global::LinqSTG.Kinematics;
using NodeNetwork;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    public class LinqSTGNodeOutputViewModel<T> : NodeOutputViewModel, IValueNodeOutput<T>
    {
        static LinqSTGNodeOutputViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeOutputView(), typeof(IViewFor<LinqSTGNodeOutputViewModel<T>>));
        }

        #region Value
        /// <summary>
        /// Observable that produces the value every time it changes.
        /// </summary>
        public IObservable<T>? Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
        private IObservable<T>? _value;
        #endregion

        #region CurrentValue
        /// <summary>
        /// The latest value produced by this output.
        /// </summary>
        public T CurrentValue => _currentValue.Value;
        private readonly ObservableAsPropertyHelper<T> _currentValue;
        #endregion

#pragma warning disable CS8618
        public LinqSTGNodeOutputViewModel()
#pragma warning restore CS8618
        {
            this.WhenAnyObservable(vm => vm.Value).ToProperty(this, vm => vm.CurrentValue, out _currentValue, false, Scheduler.Immediate);
        }
    }

    public static class LinqSTGNodeOutputViewModel
    {
        public static LinqSTGNodeOutputViewModel<Contextual<int>> Int(string? name = null, ValueEditorViewModel<Contextual<int>>? editor = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<int>>()
            {
                Name = name,
                Editor = editor,
                Value = editor?.ValueChanged,
                Port = new LinqSTGPortViewModel(PortColor.Int),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<float>> Float(string? name = null, ValueEditorViewModel<Contextual<float>>? editor = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<float>>()
            {
                Name = name,
                Editor = editor,
                Value = editor?.ValueChanged,
                Port = new LinqSTGPortViewModel(PortColor.Float),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<string>> String(string? name = null, ValueEditorViewModel<Contextual<string>>? editor = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<string>>()
            {
                Name = name,
                Editor = editor,
                Value = editor?.ValueChanged,
                Port = new LinqSTGPortViewModel(PortColor.String),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<RepeaterKey>> RepeaterKey(string? name = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<RepeaterKey>>()
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.RepeaterKey),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<Repeater>> Repeater(string? name = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<Repeater>>()
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Repeater),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<Vector2>> Vector2(string? name = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<Vector2>>()
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Vector2),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<Parameter>> Transformation(string? name = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<Parameter>>()
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Transformation),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> Pattern(string? name = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>>()
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Pattern),
            };
        }

        public static LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> Movement(string? name = null)
        {
            return new LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>>()
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Movement),
            };
        }

        public static ContextAwareNodeOutputViewModel Numeric(string? name = null)
        {
            return new ContextAwareNodeOutputViewModel()
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Numeric),
            };
        }
    }
}
