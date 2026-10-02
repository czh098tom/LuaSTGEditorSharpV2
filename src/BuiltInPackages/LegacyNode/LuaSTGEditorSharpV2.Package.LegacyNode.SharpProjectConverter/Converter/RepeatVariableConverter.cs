using System.Globalization;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>
/// Converts the V1 <c>.General.Repeat</c> variable list into the V2 node's zero based keys.
/// V1 names the slots <c>Var n name</c> / <c>Var n init value</c> / <c>Var n increment</c> with n
/// starting at 1 and reads the slot count from <c>Number of Var</c>, while the V2 list generator
/// reads <c>count</c> and <c>name{n}</c> / <c>init{n}</c> / <c>increment{n}</c>.  V1 skips a slot
/// whose name is empty, so only the declared slots are moved.
/// </summary>
public sealed class RepeatVariableConverter : ISharpNodeFormatConverter
{
    // V1 AppConstants.mxUAttr clamps the effective slot count to this limit.
    private const int MaximumLegacyCount = 100;

    public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
    {
        var properties = source.Properties;
        var count = int.TryParse(source.GetProperty("number_of_var"), out var parsed)
            ? Math.Clamp(parsed, 0, MaximumLegacyCount) : 0;
        properties.Remove("number_of_var");
        properties["count"] = count.ToString(CultureInfo.InvariantCulture);

        for (var index = 0; index < count; index++)
        {
            var legacyIndex = (index + 1).ToString(CultureInfo.InvariantCulture);
            var targetIndex = index.ToString(CultureInfo.InvariantCulture);
            Move($"var_{legacyIndex}_name", $"name_{targetIndex}");
            Move($"var_{legacyIndex}_init_value", $"init_{targetIndex}");
            Move($"var_{legacyIndex}_increment", $"increment_{targetIndex}");
        }
        return source;

        void Move(string legacyKey, string targetKey)
        {
            if (properties.Remove(legacyKey, out var value))
            {
                properties[targetKey] = value;
            }
        }
    }
}
