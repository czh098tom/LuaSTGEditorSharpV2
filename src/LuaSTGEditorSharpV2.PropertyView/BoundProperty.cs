using System;
using System.ComponentModel;

namespace LuaSTGEditorSharpV2.PropertyView;

public class BoundProperty: INotifyPropertyChanged
{
    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            PropertyChanged?.Invoke(this, ValueChangedEventArgs);
            EditRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetValueWithoutPushingCommand(string value)
    {
        _value = value;
        PropertyChanged?.Invoke(this, ValueChangedEventArgs);
    }

    private bool _hasConflict;
    public bool HasConflict
    {
        get => _hasConflict;
        set
        {
            if (_hasConflict == value)
            {
                return;
            }

            _hasConflict = value;
            if (value && _value.Length > 0)
            {
                SetValueWithoutPushingCommand(string.Empty);
            }
            PropertyChanged?.Invoke(this, ConflictChangedEventArgs);
        }
    }

    public event EventHandler? EditRequested;
    public event PropertyChangedEventHandler? PropertyChanged;
    private static readonly PropertyChangedEventArgs ValueChangedEventArgs = new (nameof(Value));
    private static readonly PropertyChangedEventArgs ConflictChangedEventArgs = new (nameof(HasConflict));
}
