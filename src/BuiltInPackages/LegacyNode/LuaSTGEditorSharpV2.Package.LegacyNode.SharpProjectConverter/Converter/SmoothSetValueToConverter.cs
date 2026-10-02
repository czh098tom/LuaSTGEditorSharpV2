using System.Globalization;
using System.Text;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>
/// Converts the V1 <c>.Task.SmoothSetValueTo</c> attribute list into the V2 node's zero based keys.
/// V1 names the items <c>Value n name</c> / <c>Target value n</c> / ... with n starting at 1, while
/// the V2 list generator reads <c>variable_name0</c> / <c>target_value0</c> / ... , and it reads the
/// item count from <c>count</c> rather than <c>Number of Value</c>.
/// </summary>
public sealed class SmoothSetValueToConverter : ISharpNodeFormatConverter
{
    // V1 AppConstants.mxUAttr and SmoothSetValueTo clamp the effective item count to this limit.
    private const int MaximumLegacyCount = 100;

    public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
    {
        var properties = source.Properties;
        var count = int.TryParse(source.GetProperty("number_of_value"), out var parsed)
            ? Math.Clamp(parsed, 0, MaximumLegacyCount) : 0;
        properties.Remove("number_of_value");
        properties["count"] = count.ToString(CultureInfo.InvariantCulture);
        var delay = source.GetProperty("waiting_time");
        properties.Remove("waiting_time");
        properties["delay"] = string.IsNullOrEmpty(delay) ? "0" : delay;
        var duration = source.GetProperty("duration");
        properties["duration"] = string.IsNullOrEmpty(duration) ? "1" : duration;

        for (var index = 0; index < MaximumLegacyCount; index++)
        {
            var legacyIndex = (index + 1).ToString(CultureInfo.InvariantCulture);
            var targetIndex = index.ToString(CultureInfo.InvariantCulture);
            Move($"value_{legacyIndex}_name", $"variable_name{targetIndex}", index < count, quote: true);
            Move($"target_value_{legacyIndex}", $"target_value{targetIndex}", index < count, quote: false);
            Move($"interpolation_mode_{legacyIndex}", $"interpolation_mode{targetIndex}", index < count, quote: false);
            Move($"modification_mode_{legacyIndex}", $"modification_mode{targetIndex}", index < count, quote: false);
        }
        return source;

        void Move(string legacyKey, string targetKey, bool isActiveItem, bool quote)
        {
            string value;
            if (!properties.Remove(legacyKey, out value!))
            {
                if (!isActiveItem) return;
                value = string.Empty;
            }
            // V1 emits the property name as a Lua literal and escapes it with StringParser.ParseLua;
            // the other three slots stay expressions.
            properties[targetKey] = quote ? AsLuaStringLiteral(value) : value;
        }
    }

    private static string AsLuaStringLiteral(string value)
    {
        if (value.Length >= 2 && (value[0] == '"' || value[0] == '\'') && value[^1] == value[0])
        {
            return value;
        }
        var builder = new StringBuilder(value.Length + 2).Append('"');
        foreach (var character in value)
        {
            if (character is '\\' or '\'' or '"') builder.Append('\\');
            builder.Append(character);
        }
        return builder.Append('"').ToString();
    }
}
