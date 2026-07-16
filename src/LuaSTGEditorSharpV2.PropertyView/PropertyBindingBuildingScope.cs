using System;
using System.Collections.Generic;
using System.Linq;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView;

public class PropertyBindingBuildingScope<TTerm>(NodePropertyCapture nodeProperty, BoundPropertyItemViewModelBase<TTerm> vm,
        Action<string, PropertyBinding> addBindingCallback)
    where TTerm: class
{
    public void ToOne(BoundProperty boundProperty, Func<string, string>? convert, Func<string, string>? convertBack)
    {
        PropertyBinding binding = new(
            Capture: nodeProperty,
            BoundProperties: [boundProperty],
            PullAction: value => boundProperty.SetValueWithoutPushingCommand(convertBack?.Invoke(value) ?? value),
            EditResultResolver: () => new EditResult(
                Commands.FromEnumerable(vm.SourceNodes.Select(
                    n => CheckedCommand.Property.Modify(n.Document, n.GetPath(), nodeProperty.Key, convert?.Invoke(boundProperty.Value) ?? boundProperty.Value))),
                vm.LocalServiceParam));
        addBindingCallback.Invoke(nodeProperty.Key, binding);
        boundProperty.EditRequested += (sender, args) =>
        {
            vm.RaiseOnEdit(binding.EditResultResolver.Invoke());
        };
    }

    public void ToOne(BoundProperty boundProperty)
        => ToOne(boundProperty, null, null);

    public void ToMany(IReadOnlyList<BoundProperty> boundProperties, Func<string[], string> compose, Func<string, string[]> decompose)
    {
        PropertyBinding binding = new(
            Capture: nodeProperty,
            BoundProperties: boundProperties,
            PullAction: value =>
            {
                var decomposed = decompose.Invoke(value);
                for (var i = 0; i < boundProperties.Count; i++)
                {
                    boundProperties[i].SetValueWithoutPushingCommand(decomposed[i]);
                }
            },
            EditResultResolver: () => new EditResult(
                Commands.FromEnumerable(vm.SourceNodes.Select(n => CheckedCommand.Property.Modify(n.Document,
                    n.GetPath(), nodeProperty.Key,
                    compose(boundProperties.Select(bp => bp.Value).ToArray())))),
                vm.LocalServiceParam));
        addBindingCallback.Invoke(nodeProperty.Key, binding);
        foreach (var boundProperty in boundProperties)
        {
            boundProperty.EditRequested += (sender, args) =>
            {
                vm.RaiseOnEdit(binding.EditResultResolver.Invoke());
            };
        }
    }

    public void ToMany(
        (BoundProperty, BoundProperty) boundProperties,
        Func<string, string, string> compose,
        Func<string, (string, string)> decompose)
    {
        ToMany(
            [boundProperties.Item1, boundProperties.Item2],
            values => compose(values[0], values[1]),
            value =>
            {
                var values = decompose(value);
                return [values.Item1, values.Item2];
            });
    }

    public void ToMany(
        (BoundProperty, BoundProperty, BoundProperty) boundProperties,
        Func<string, string, string, string> compose,
        Func<string, (string, string, string)> decompose)
    {
        ToMany(
            [boundProperties.Item1, boundProperties.Item2, boundProperties.Item3],
            values => compose(values[0], values[1], values[2]),
            value =>
            {
                var values = decompose(value);
                return [values.Item1, values.Item2, values.Item3];
            });
    }

    public void ToMany(
        (BoundProperty, BoundProperty, BoundProperty, BoundProperty) boundProperties,
        Func<string, string, string, string, string> compose,
        Func<string, (string, string, string, string)> decompose)
    {
        ToMany(
            [boundProperties.Item1, boundProperties.Item2, boundProperties.Item3, boundProperties.Item4],
            values => compose(values[0], values[1], values[2], values[3]),
            value =>
            {
                var values = decompose(value);
                return [values.Item1, values.Item2, values.Item3, values.Item4];
            });
    }
}
