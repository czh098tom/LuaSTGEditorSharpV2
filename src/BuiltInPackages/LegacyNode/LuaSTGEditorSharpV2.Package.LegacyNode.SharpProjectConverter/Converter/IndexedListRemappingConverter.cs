using System.Globalization;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>Reindexes the bounded property lists used by V1 nodes without compacting slots.</summary>
[JsonTypeShortName(typeof(ISharpNodeFormatConverter), "IndexedListRemapping")]
public sealed class IndexedListRemappingConverter : ISharpNodeFormatConverter
{
    [JsonProperty(Required = Required.Always)] public string OriginalCountKey { get; private set; } = string.Empty;
    [JsonProperty(Required = Required.Always)] public string NewCountKey { get; private set; } = string.Empty;
    [JsonProperty] public int OriginalIndexBase { get; private set; } = 1;
    [JsonProperty] public int NewIndexBase { get; private set; }
    [JsonProperty] public int MaximumCount { get; private set; } = 100;
    [JsonProperty(Required = Required.Always)] public IndexedPropertyRemappingRule[] PropertyRules { get; private set; } = [];
    [JsonProperty] public ISharpNodeFormatConverter[] ItemConverters { get; private set; } = [];
    [JsonProperty] public bool PreserveInactiveItems { get; private set; }

    public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
    {
        ValidateConfiguration();
        var original = new Dictionary<string, string>(source.Properties, StringComparer.Ordinal);
        var count = int.TryParse(original.GetValueOrDefault(OriginalCountKey), out var parsed)
            ? Math.Clamp(parsed, 0, MaximumCount) : 0;
        var slots = PreserveInactiveItems ? MaximumCount : count;
        var sourceKeys = new HashSet<string>(StringComparer.Ordinal) { OriginalCountKey };
        var targetKeys = new HashSet<string>(StringComparer.Ordinal) { NewCountKey };
        var itemKeys = PropertyRules.Select(rule => rule.ItemKey).ToHashSet(StringComparer.Ordinal);
        var removals = new HashSet<string>(StringComparer.Ordinal) { OriginalCountKey };
        var additions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [NewCountKey] = count.ToString(CultureInfo.InvariantCulture)
        };

        // Stage all reads and transformations before touching the source dictionary.
        for (var index = 0; index < slots; index++)
        {
            var item = new NodeData(source.TypeUID) { IsActive = source.IsActive };
            var newKeys = new string[PropertyRules.Length];
            for (var column = 0; column < PropertyRules.Length; column++)
            {
                var rule = PropertyRules[column];
                var oldKey = FormatKey(rule.OriginalKeyFormat, OriginalIndexBase + index);
                var newKey = FormatKey(rule.NewKeyFormat, NewIndexBase + index);
                if (!sourceKeys.Add(oldKey) || !targetKeys.Add(newKey))
                {
                    throw Error(source, $"PropertyRules contain overlapping keys at slot {index}.");
                }
                newKeys[column] = newKey;
                if (original.TryGetValue(oldKey, out var value))
                {
                    removals.Add(oldKey);
                    item.Properties.Add(rule.ItemKey, value);
                }
                else if (index < count && rule.DefaultValue is not null)
                {
                    item.Properties.Add(rule.ItemKey, rule.DefaultValue);
                }
            }
            if (index >= count && item.Properties.Count == 0)
            {
                continue;
            }
            var presentItemKeys = item.Properties.Keys.ToHashSet(StringComparer.Ordinal);
            foreach (var converter in ItemConverters)
            {
                var itemContext = new SharpNodeFormattingContext();
                var result = converter.Convert(item, itemContext);
                if (!ReferenceEquals(result, item) || item.TypeUID != source.TypeUID
                    || item.IsActive != source.IsActive || item.PhysicalParent is not null
                    || item.PhysicalChildren.Count != 0 || itemContext.ShouldRetry
                    || item.Properties.Keys.Any(key => !itemKeys.Contains(key)))
                {
                    throw Error(source, "ItemConverters may only transform the configured item properties.");
                }
            }
            for (var column = 0; column < PropertyRules.Length; column++)
            {
                if ((index < count || presentItemKeys.Contains(PropertyRules[column].ItemKey))
                    && item.Properties.TryGetValue(PropertyRules[column].ItemKey, out var value))
                {
                    additions.Add(newKeys[column], value);
                }
            }
        }
        foreach (var key in additions.Keys)
        {
            if (original.ContainsKey(key) && !removals.Contains(key))
            {
                throw Error(source, $"Target property '{key}' already exists.");
            }
        }
        foreach (var key in removals)
        {
            source.Properties.Remove(key);
        }
        foreach (var entry in additions)
        {
            source.Properties.Add(entry.Key, entry.Value);
        }
        return source;
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(OriginalCountKey) || string.IsNullOrEmpty(NewCountKey))
        {
            throw new InvalidOperationException("IndexedListRemapping count keys must not be empty.");
        }
        // V1 caps every list in this converter's domain at 100 physical slots.
        if (MaximumCount is < 0 or > 100 || OriginalIndexBase < 0 || NewIndexBase < 0
            || OriginalIndexBase > int.MaxValue - MaximumCount
            || NewIndexBase > int.MaxValue - MaximumCount)
        {
            throw new InvalidOperationException("IndexedListRemapping index bases or MaximumCount are invalid.");
        }
        if (PropertyRules is null || PropertyRules.Length == 0 || ItemConverters is null
            || ItemConverters.Any(converter => converter is null))
        {
            throw new InvalidOperationException("IndexedListRemapping requires PropertyRules and non-null ItemConverters.");
        }
        var itemKeys = new HashSet<string>(StringComparer.Ordinal);
        var sourceKeys = new HashSet<string>(StringComparer.Ordinal) { OriginalCountKey };
        var targetKeys = new HashSet<string>(StringComparer.Ordinal) { NewCountKey };
        foreach (var rule in PropertyRules)
        {
            if (rule is null || string.IsNullOrEmpty(rule.ItemKey) || !itemKeys.Add(rule.ItemKey)
                || string.IsNullOrEmpty(rule.OriginalKeyFormat) || string.IsNullOrEmpty(rule.NewKeyFormat))
            {
                throw new InvalidOperationException("IndexedListRemapping PropertyRules must have unique, non-empty item keys and formats.");
            }
            for (var index = 0; index < MaximumCount; index++)
            {
                if (!sourceKeys.Add(FormatKey(rule.OriginalKeyFormat, OriginalIndexBase + index))
                    || !targetKeys.Add(FormatKey(rule.NewKeyFormat, NewIndexBase + index)))
                {
                    throw new InvalidOperationException("IndexedListRemapping PropertyRules expand to overlapping keys.");
                }
            }
        }
    }

    private static string FormatKey(string format, int index)
        => string.Format(CultureInfo.InvariantCulture, format, index);

    private static InvalidOperationException Error(NodeData source, string message)
        => new($"IndexedListRemapping on '{source.TypeUID}': {message}");
}
