using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView;

public record PropertySource(
    EditorNode Node,
    NodePropertyAccessToken Token);