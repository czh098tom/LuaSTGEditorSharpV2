using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView;

public class PropertyBindingBuildingScope<TTerm>(
        NodePropertyCapture nodeProperty,
        bool shouldRefreshView,
        Action<string, PropertyBinding> addBindingCallback)
    where TTerm: class
{
    public void ToOne(BoundProperty boundProperty, Func<string, string>? convert, Func<string, string>? convertBack)
    {
        PropertyBinding binding = new(
            Capture: nodeProperty,
            BoundProperties: [boundProperty],
            Decompose: value => [convertBack?.Invoke(value) ?? value],
            Compose: values => convert?.Invoke(values[0]) ?? values[0],
            ShouldRefreshView: shouldRefreshView);
        addBindingCallback.Invoke(nodeProperty.Key, binding);
    }

    public void ToOne(BoundProperty boundProperty)
        => ToOne(boundProperty, null, null);

    public void ToMany(IReadOnlyList<BoundProperty> boundProperties, Func<string[], string> compose, Func<string, string[]> decompose)
    {
        PropertyBinding binding = new(
            Capture: nodeProperty,
            BoundProperties: boundProperties,
            Decompose: decompose,
            Compose: compose,
            ShouldRefreshView: shouldRefreshView);
        addBindingCallback.Invoke(nodeProperty.Key, binding);
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
