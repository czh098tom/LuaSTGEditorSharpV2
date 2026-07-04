using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using global::LinqSTG.Kinematics;
using NodeNetwork;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    public class LinqSTGNodeInputViewModel<T> : NodeInputViewModel
    {
        static LinqSTGNodeInputViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeInputView(), typeof(IViewFor<LinqSTGNodeInputViewModel<T>>));
        }

        #region Value
        /// <summary>
        /// The value currently associated with this input.
        /// If the input is not connected, the value is taken from ValueEditorViewModel.Value in the Editor property.
        /// If the input is connected, the value is taken from ValueNodeOutputViewModel.LatestValue unless the network is not traversable.
        /// Note that this value may be equal to default(T) if there is an error somewhere.
        /// </summary>
        public T? Value => _value.Value;
        private readonly ObservableAsPropertyHelper<T?> _value;
        #endregion

        #region ValueChanged
        /// <summary>
        /// An observable that fires when the input value changes. 
        /// This may be because of a connection change, editor value change, network validation change, ...
        /// </summary>
        public IObservable<T?> ValueChanged { get; }
        #endregion

        /// <summary>
        /// Constructs a new ValueNodeInputViewModel with the specified ValidationActions. 
        /// The default values are carefully chosen and should probably not be changed unless you know what you are doing.
        /// </summary>
        /// <param name="connectionChangedValidationAction">The validation behaviour when the connection of this input changes.</param>
        /// <param name="connectedValueChangedValidationAction">The validation behaviour when the value of this input changes.</param>
        public LinqSTGNodeInputViewModel(
            ValidationAction connectionChangedValidationAction = ValidationAction.PushDefaultValue,
            ValidationAction connectedValueChangedValidationAction = ValidationAction.IgnoreValidation)
        {
            MaxConnections = 1;
            ConnectionValidator = pending =>
            {
                var sameType = pending.Output is LinqSTGNodeOutputViewModel<T>;
                if (pending.Output is ContextAwareNodeOutputViewModel ctx)
                {
                    sameType = ctx.CurrentType == typeof(T);
                }
                return new ConnectionValidationResult(sameType, null);
            };

            var connectedValues = GenerateConnectedValuesBinding(connectionChangedValidationAction, connectedValueChangedValidationAction);

            var localValues = this.WhenAnyValue(vm => vm.Editor)
                .Select(e =>
                {
                    if (e == null)
                    {
                        return Observable.Return(default(T));
                    }
                    else if (!(e is ValueEditorViewModel<T>))
                    {
                        throw new Exception($"The endpoint editor is not a subclass of ValueEditorViewModel<{typeof(T).Name}>");
                    }
                    else
                    {
                        return ((ValueEditorViewModel<T>)e).ValueChanged;
                    }
                })
                .Switch();

            var valueChanged = Observable.CombineLatest(connectedValues, localValues,
                    (connectedValue, localValue) => Connections.Count == 0 ? localValue : connectedValue
                ).Publish();
            valueChanged.Connect();
            valueChanged.ToProperty(this, vm => vm.Value, out _value);

            ValueChanged = Observable
                .Defer(() => Observable.Return(Value))
                .Concat(valueChanged);
        }

        private IObservable<T?> GenerateConnectedValuesBinding(ValidationAction connectionChangedValidationAction, ValidationAction connectedValueChangedValidationAction)
        {
            var onConnectionChanged = this.Connections.Connect().Select(_ => Unit.Default).StartWith(Unit.Default)
                .Select(_ => Connections.Count == 0 ? null : Connections.Items.First());

            //On connection change
            IObservable<IObservable<T?>> connectionObservables;
            if (connectionChangedValidationAction != ValidationAction.DontValidate)
            {
                //Either run network validation
                IObservable<NetworkValidationResult> postValidation = onConnectionChanged
                    .SelectMany(con => Parent?.Parent?.UpdateValidation.Execute() ?? Observable.Return(new NetworkValidationResult(true, true, null)));

                if (connectionChangedValidationAction == ValidationAction.WaitForValid)
                {
                    //And wait until the validation is successful
                    postValidation = postValidation.SelectMany(validation =>
                        validation.NetworkIsTraversable
                            ? Observable.Return(validation)
                            : Parent.Parent.Validation.FirstAsync(val => val.NetworkIsTraversable));
                }

                if (connectionChangedValidationAction == ValidationAction.PushDefaultValue)
                {
                    //Or push a single default(T) if the validation fails
                    connectionObservables = postValidation.Select(validation =>
                    {
                        if (Connections.Count == 0)
                        {
                            return Observable.Return(default(T));
                        }
                        else if (validation.NetworkIsTraversable)
                        {
                            IObservable<T>? connectedObservable =
                                ((IValueNodeOutput<object>)Connections.Items.First().Output).Value
                                    ?.OfType<T>();
                            if (connectedObservable == null)
                            {
                                throw new Exception($"The value observable for output '{Connections.Items.First().Output.Name}' is null.");
                            }
                            return connectedObservable;
                        }
                        else
                        {
                            return Observable.Return(default(T));
                        }
                    });
                }
                else
                {
                    //Grab the values observable from the connected output
                    connectionObservables = postValidation
                        .Select(_ =>
                        {
                            if (Connections.Count == 0)
                            {
                                return Observable.Return(default(T));
                            }
                            else
                            {
                                IObservable<T>? connectedObservable =
                                    ((IValueNodeOutput<object>)Connections.Items.First().Output).Value
                                        ?.OfType<T>();
                                if (connectedObservable == null)
                                {
                                    throw new Exception($"The value observable for output '{Connections.Items.First().Output.Name}' is null.");
                                }
                                return connectedObservable;
                            }
                        });
                }
            }
            else
            {
                //Or just grab the values observable from the connected output
                connectionObservables = onConnectionChanged.Select(con =>
                {
                    if (con == null)
                    {
                        return Observable.Return(default(T));
                    }
                    else
                    {
                        IObservable<T>? connectedObservable =
                            ((IValueNodeOutput<object>)con.Output).Value
                                ?.OfType<T>();
                        if (connectedObservable == null)
                        {
                            throw new Exception($"The value observable for output '{Connections.Items.First().Output.Name}' is null.");
                        }
                        return connectedObservable;
                    }
                });
            }
            IObservable<T?> connectedValues = connectionObservables.SelectMany(c => c);

            //On connected output value change, either just push the value as is
            if (connectedValueChangedValidationAction != ValidationAction.DontValidate)
            {
                //Or run a network validation
                IObservable<NetworkValidationResult> postValidation = connectedValues.SelectMany(v =>
                    Parent?.Parent?.UpdateValidation.Execute() ?? Observable.Return(new NetworkValidationResult(true, true, null)));
                if (connectedValueChangedValidationAction == ValidationAction.WaitForValid)
                {
                    //And wait until the validation is successful
                    postValidation = postValidation.SelectMany(validation =>
                        validation.IsValid
                            ? Observable.Return(validation)
                            : Parent.Parent.Validation.FirstAsync(val => val.IsValid));
                }

                connectedValues = postValidation.Select(validation =>
                {
                    if (Connections.Count == 0
                        || connectionChangedValidationAction == ValidationAction.PushDefaultValue && !validation.NetworkIsTraversable
                        || connectedValueChangedValidationAction == ValidationAction.PushDefaultValue && !validation.IsValid)
                    {
                        //Push default(T) if the network isn't valid
                        return default;
                    }

                    //Or just ignore the validation and push the value as is
                    return ((IValueNodeOutput<object>)this.Connections.Items.First().Output).CurrentValue 
                        is T result ? result : default;
                });
            }

            return connectedValues;
        }
    }

    public static class LinqSTGNodeInputViewModel
    {
        public static LinqSTGNodeInputViewModel<Contextual<int>?> Int(string? name = null, ValueEditorViewModel<Contextual<int>>? editor = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<int>?>
            {
                Name = name,
                Editor = editor,
                Port = new LinqSTGPortViewModel(PortColor.Int),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<float>?> Float(string? name = null, ValueEditorViewModel<Contextual<float>>? editor = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<float>?>
            {
                Name = name,
                Editor = editor,
                Port = new LinqSTGPortViewModel(PortColor.Float),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<string>?> String(string? name = null, ValueEditorViewModel<Contextual<string>>? editor = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<string>?>
            {
                Name = name,
                Editor = editor,
                Port = new LinqSTGPortViewModel(PortColor.String),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<RepeaterKey>?> RepeaterKey(string? name = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<RepeaterKey>?>
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.RepeaterKey),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<Repeater>?> Repeater(string? name = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<Repeater>?>
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Repeater),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<IntervalType>?> IntervalType(string? name = null, ValueEditorViewModel<Contextual<IntervalType>>? editor = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<IntervalType>?>
            {
                Name = name,
                Editor = editor,
                Port = null,
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<Vector2>?> Vector2(string? name = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<Vector2>?>
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Vector2),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<Parameter>?> Transformation(string? name = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<Parameter>?>
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Transformation),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> Pattern(string? name = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?>
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Pattern),
            };
        }

        public static LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> Movement(string? name = null)
        {
            return new LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?>
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Movement),
            };
        }

        public static ContextAwareNodeInputViewModel Numeric(string? name = null)
        {
            return new ContextAwareNodeInputViewModel([
                typeof(Contextual<float>),
                typeof(Contextual<int>),
                typeof(Contextual<Vector2>)])
            {
                Name = name,
                Port = new LinqSTGPortViewModel(PortColor.Numeric),
            };
        }
    }
}
