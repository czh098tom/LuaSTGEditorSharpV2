using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel;

public sealed class UnsupportedMultiSourcePropertyItemViewModel : PropertyItemViewModelBase
{
    public UnsupportedMultiSourcePropertyItemViewModel(string value)
    {
        Value = value;
        Enabled = false;
        Type = new PropertyViewEditorType("captionValue");
    }

    public string Value { get; }

    protected override void HandleEditorNodeOnPropertyChanged(
        object? sender,
        EditorNodePropertyChangedEventArgs e)
    {
    }
}
