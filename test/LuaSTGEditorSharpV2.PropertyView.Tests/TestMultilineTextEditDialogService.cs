namespace LuaSTGEditorSharpV2.PropertyView.Tests;

internal sealed class TestMultilineTextEditDialogService(string? result)
    : IMultilineTextEditDialogService
{
    public string? RequestedTitle { get; private set; }
    public string? InitialValue { get; private set; }

    public string? EditText(string title, string initialValue)
    {
        RequestedTitle = title;
        InitialValue = initialValue;
        return result;
    }
}
