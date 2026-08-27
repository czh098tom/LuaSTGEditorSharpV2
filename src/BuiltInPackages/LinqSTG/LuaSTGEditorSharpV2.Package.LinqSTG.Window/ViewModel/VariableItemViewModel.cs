using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// A single entry of the blueprint window's variable list.
    /// All mutations are routed through the owning <see cref="VariableListViewModel"/>
    /// (set at construction) so uniqueness, locking and change notifications are
    /// enforced in one place; the property setters below are the entry points
    /// used by the two-way bindings of the list's text boxes.
    /// </summary>
    public class VariableItemViewModel : INotifyPropertyChanged
    {
        private readonly VariableListViewModel _owner;
        private string _name = string.Empty;
        private string _valueText = "0";
        private double _value;
        private bool _isInteger;
        private bool _isLocked;

        public event PropertyChangedEventHandler? PropertyChanged;

        internal VariableItemViewModel(VariableListViewModel owner)
        {
            _owner = owner;
        }

        /// <summary>The unique name; duplicates committed through <see cref="Name"/> are forced to _{n}.</summary>
        public string Name
        {
            get => _name;
            set => _owner.RenameItem(this, value);
        }

        /// <summary>The committed numeric value.</summary>
        public double Value => _value;

        /// <summary>
        /// The text being edited for the value. Assignments are parsed per
        /// <see cref="IsInteger"/>; text that fails to parse reverts to the
        /// last valid representation.
        /// </summary>
        public string ValueText
        {
            get => _valueText;
            set => _owner.CommitValue(this, value);
        }

        /// <summary>Whether the value is authored as an integer instead of a float.</summary>
        public bool IsInteger
        {
            get => _isInteger;
            set => _owner.SetInteger(this, value);
        }

        /// <summary>True while the item sits within the list's locked prefix: fixed position, no rename, no delete.</summary>
        public bool IsLocked
        {
            get => _isLocked;
            internal set => SetField(ref _isLocked, value);
        }

        internal void SetNameInternal(string name)
        {
            SetField(ref _name, name);
        }

        internal void SetValueInternal(double value, string valueText)
        {
            _value = value;
            SetField(ref _valueText, valueText);
        }

        internal void SetIntegerInternal(bool isInteger)
        {
            SetField(ref _isInteger, isInteger);
        }

        /// <summary>Re-raises <see cref="Name"/> so an in-progress edit snaps back to the canonical text.</summary>
        internal void NotifyNameReset()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }

        /// <summary>Re-raises <see cref="ValueText"/> so unparseable edits snap back to the last valid text.</summary>
        internal void NotifyValueReset()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueText)));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string caller = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
