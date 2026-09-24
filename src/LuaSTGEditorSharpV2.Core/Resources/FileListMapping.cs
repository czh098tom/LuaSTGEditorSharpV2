using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.Resources;

public sealed class FileListMapping
{
    [JsonProperty]
    public NodePropertyCapture Count { get; private set; } = NodePropertyCapture.FromKey("count", "0");

    [JsonProperty]
    public NodePropertyCapture Source { get; private set; } = NodePropertyCapture.FromKey("source_{0}");

    [JsonProperty]
    public NodePropertyCapture Destination { get; private set; } = NodePropertyCapture.FromKey("destination_{0}");

    public FileListMapping WithCount(NodePropertyCapture count)
        => new() { Count = count, Source = Source, Destination = Destination };

    public static bool TryParseCount(string value, out int count)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out count) && count >= 0;

    public int GetCount(NodePropertyAccessToken token)
    {
        var value = Count.Capture(token);
        if (!TryParseCount(value, out var count))
        {
            throw new InvalidOperationException($"Invalid file count '{value}' on '{token.NodeData.TypeUID}'.");
        }
        return count;
    }

    public IReadOnlyList<FileListItemMapping> GetItems(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (string.IsNullOrWhiteSpace(Count.Key)
            || Source.Format(0).Key == Source.Format(1).Key
            || Destination.Format(0).Key == Destination.Format(1).Key)
        {
            throw new InvalidOperationException("File lists require a count key and indexed source/destination mappings.");
        }

        var keys = new HashSet<string>(StringComparer.Ordinal) { Count.Key };
        var items = new List<FileListItemMapping>();
        for (var index = 0; index < count; index++)
        {
            var item = new FileListItemMapping(index, Source.Format(index), Destination.Format(index));
            if (string.IsNullOrWhiteSpace(item.Source.Key) || string.IsNullOrWhiteSpace(item.Destination.Key)
                || !keys.Add(item.Source.Key) || !keys.Add(item.Destination.Key))
            {
                throw new InvalidOperationException("File list property mappings overlap or are empty.");
            }
            items.Add(item);
        }
        return items;
    }
}

public sealed record FileListItemMapping(
    int Index,
    NodePropertyCapture Source,
    NodePropertyCapture Destination);
