using System.Globalization;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>Normalizes RecordPos's positional array before repeated captions reach a dictionary.</summary>
public static class RecordPosConverter
{
    public static IEnumerable<SharpAttribute?> NormalizeAttributes(SharpNode source)
    {
        var attributes = source.SharpAttributes ?? [];
        var type = SharpResourceMigrationAnalyzer.GetLegacyTypeName(new NodeData(source.Type ?? string.Empty));
        if (type != "RecordPos")
        {
            return attributes;
        }
        if (attributes.Length == 0 || attributes.Length % 2 == 0)
        {
            throw Error(source, "Expected a count followed by complete name/object pairs.");
        }
        if (attributes.Any(attribute => attribute is null))
        {
            throw Error(source, "The positional attribute array contains a null slot.");
        }
        // Validate count against physical slots, but never allocate by an untrusted count.
        var count = int.TryParse(attributes[0]!.AttrInput, out var parsed) ? Math.Clamp(parsed, 0, 100) : 0;
        if (count > (attributes.Length - 1) / 2)
        {
            throw Error(source, $"Count {count} exceeds the available physical pairs.");
        }
        var result = new SharpAttribute?[attributes.Length];
        result[0] = Copy(attributes[0]!, "Number of Var");
        for (var slot = 1; slot < attributes.Length; slot += 2)
        {
            var index = ((slot + 1) / 2).ToString(CultureInfo.InvariantCulture);
            result[slot] = Copy(attributes[slot]!, $"Var {index} name");
            result[slot + 1] = Copy(attributes[slot + 1]!, $"Var {index} object");
        }
        return result;
    }

    private static SharpAttribute Copy(SharpAttribute source, string caption)
        => new() { AttrCap = caption, AttrInput = source.AttrInput };

    private static InvalidOperationException Error(SharpNode source, string message)
        => new($"RecordPos '{source.Type}': {message}");
}
