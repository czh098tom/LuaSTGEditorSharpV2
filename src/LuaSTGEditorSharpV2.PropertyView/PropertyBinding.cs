using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView;

public record struct PropertyBinding(
    IReadOnlyList<BoundProperty> BoundProperties,
    NodePropertyCapture Capture,
    Action<string> PullAction,
    Func<EditResult> EditResultResolver
)
{
    private bool _hasConflict;
    public bool HasConflict
    {
        get => _hasConflict;
        set
        {
            _hasConflict = value;
            foreach (var boundProperty in BoundProperties)
            {
                boundProperty.HasConflict = value;
            }
        }
    }
};