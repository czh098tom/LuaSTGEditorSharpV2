using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class PropertyItemViewModelBase : BaseViewModel
    {
        public class ValueUpdatedEventArgs(string old, string @new) : EventArgs
        {
            public string OldValue { get; private set; } = old;
            public string NewValue { get; private set; } = @new;
        }

        private string _value = string.Empty;
        private PropertyViewEditorType? _type;

        public string Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                _value = value;
                RaisePropertyChanged();
                OnValueUpdated?.Invoke(this, new ValueUpdatedEventArgs(oldValue, value));
            }
        }

        public PropertyViewEditorType? Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }

        public event EventHandler<ValueUpdatedEventArgs>? OnValueUpdated;
    }
}
