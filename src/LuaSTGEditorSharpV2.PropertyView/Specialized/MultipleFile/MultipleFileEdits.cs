using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Resources;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile;

internal static class MultipleFileEdits
{
    public static EditResult? Append(EditorNode node, NodePropertyAccessToken token,
        FileListMapping files, IReadOnlyList<string> paths, LocalServiceParam local)
    {
        var count = files.GetCount(token);
        if (paths.Count == 0)
        {
            return null;
        }
        var newCount = checked(count + paths.Count);
        var destinations = paths.Select(path =>
        {
            var name = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Each selected path must name a file.", nameof(paths));
            }
            return name;
        }).ToArray();
        var mappings = files.GetItems(newCount);
        var commands = new List<CommandBase?>();
        for (var offset = 0; offset < paths.Count; offset++)
        {
            var mapping = mappings[count + offset];
            commands.Add(CheckedCommand.Property.Modify(
                node.Document, node.GetPath(), mapping.Source.Key, paths[offset]));
            commands.Add(CheckedCommand.Property.Modify(
                node.Document, node.GetPath(), mapping.Destination.Key, destinations[offset]));
        }
        return Complete(node, files, newCount, commands, local);
    }

    public static EditResult Remove(EditorNode node, NodePropertyAccessToken token,
        FileListMapping files, int index, LocalServiceParam local)
    {
        var count = files.GetCount(token);
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        var mappings = files.GetItems(count);
        var commands = new List<CommandBase?>();
        for (var current = index; current < count - 1; current++)
        {
            commands.Add(CheckedCommand.Property.Modify(node.Document, node.GetPath(),
                mappings[current].Source.Key, mappings[current + 1].Source.Capture(token)));
            commands.Add(CheckedCommand.Property.Modify(node.Document, node.GetPath(),
                mappings[current].Destination.Key, mappings[current + 1].Destination.Capture(token)));
        }
        RemovePair(node, mappings[^1], commands);
        return Complete(node, files, count - 1, commands, local);
    }

    public static EditResult? Clear(EditorNode node, NodePropertyAccessToken token,
        FileListMapping files, LocalServiceParam local)
    {
        var count = files.GetCount(token);
        if (count == 0)
        {
            return null;
        }
        var commands = new List<CommandBase?>();
        foreach (var mapping in files.GetItems(count))
        {
            RemovePair(node, mapping, commands);
        }
        return Complete(node, files, 0, commands, local);
    }

    private static void RemovePair(EditorNode node, FileListItemMapping mapping,
        ICollection<CommandBase?> commands)
    {
        foreach (var key in new[] { mapping.Source.Key, mapping.Destination.Key })
        {
            if (node.Source.Properties.ContainsKey(key))
            {
                commands.Add(AtomicCommand.RemoveProperty(node.Document, node.GetPath(), key));
            }
        }
    }

    private static EditResult Complete(EditorNode node, FileListMapping files, int count,
        ICollection<CommandBase?> commands, LocalServiceParam local)
    {
        commands.Add(CheckedCommand.Property.Modify(node.Document, node.GetPath(),
            files.Count.Key, count.ToString(CultureInfo.InvariantCulture)));
        return new EditResult(Commands.FromEnumerable(commands), true, local);
    }
}
