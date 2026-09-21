using System;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    // Marks an evaluation failure already attributed to its originating output port.
    internal sealed class ContextualEvaluationException(string message, Exception innerException)
        : Exception(message, innerException)
    {
    }
}
