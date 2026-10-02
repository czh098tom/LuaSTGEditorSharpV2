using LuaSTGEditorSharpV2.Core.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable;

internal static class ContextCaptureSelector
{
    internal static void Validate(string? typeUID, string[]? anyOfTypeUIDs)
    {
        if ((typeUID is null) == (anyOfTypeUIDs is null))
            throw new JsonSerializationException("ContextCapture requires exactly one of TypeUID or AnyOfTypeUIDs.");
        if (typeUID is not null && string.IsNullOrWhiteSpace(typeUID))
            throw new JsonSerializationException("ContextCapture.TypeUID must not be empty or whitespace.");
        if (anyOfTypeUIDs is not null && (anyOfTypeUIDs.Length == 0 || anyOfTypeUIDs.Any(string.IsNullOrWhiteSpace)))
            throw new JsonSerializationException("ContextCapture.AnyOfTypeUIDs must contain nonempty type names.");
    }

    internal static NodeData? Resolve(NodeContext context, string? typeUID, string[]? anyOfTypeUIDs)
    {
        Validate(typeUID, anyOfTypeUIDs);
        return typeUID is not null ? context.PeekType(typeUID)
            : context.EnumerateFromNearest().FirstOrDefault(node => anyOfTypeUIDs!.Contains(node.TypeUID, StringComparer.Ordinal));
    }
}
