namespace LuaSTGEditorSharpV2.PropertyView.Tests;

internal sealed class TestCodeEditDialogService(string? result) : ICodeEditDialogService
{
    public string? Result { get; set; } = result;
    public string? RequestedTitle { get; private set; }
    public string? InitialValue { get; private set; }

    public string? EditCode(string title, string initialValue)
    {
        RequestedTitle = title;
        InitialValue = initialValue;
        return Result;
    }
}
