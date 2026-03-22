using System;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.PropertyView.Specialized.SmoothSetValueTo
{
    public record class SmoothSetValueDefinition(string VariableName, string TargetValue, string InterpolationMode, string ModificationMode);
}
