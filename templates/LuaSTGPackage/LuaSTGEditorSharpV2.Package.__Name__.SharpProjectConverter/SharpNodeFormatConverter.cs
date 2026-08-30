using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.__Name__.SharpProjectConverter;

// Placeholder converter type. Replace with your own conversion logic — a typical implementation
// reads a legacy node identifier from JSON and emits a NodeData tree in the current schema.
// Remove this file once you have a real converter, or keep it as the default fallback.
public class SharpNodeFormatConverter(IServiceProvider serviceProvider) : PackedDataBase(serviceProvider)
{
    public override string? UniqueKey => null;
}
