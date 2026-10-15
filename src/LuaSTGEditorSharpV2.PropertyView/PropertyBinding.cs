using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView;

public sealed record PropertyBinding(
    NodePropertyCapture Capture,
    IReadOnlyList<BoundProperty> BoundProperties,
    Func<string, string[]> Decompose,
    Func<string[], string> Compose,
    bool ShouldRefreshView)
{
    public string[] Pull(NodePropertyAccessToken source)
        => Decompose(Capture.Capture(source));
}
