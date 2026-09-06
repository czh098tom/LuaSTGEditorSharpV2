using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// A single entry of the blueprint window's variable list.
    /// All mutations are routed through the owning <see cref="VariableListViewModel"/>
    /// (set at construction) so uniqueness, locking and change notifications are
    /// enforced in one place; the property setters below are the entry points
    /// used by the two-way bindings of the list's text boxes and type picker.
    /// </summary>
    public class VariableItemViewModel : INotifyPropertyChanged
    {
        private readonly VariableListViewModel _owner;
        private string _name = string.Empty;
        private string _valueText = "0";
        private double _value;
        private double? _valueY;
        private int _typeIndex;
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

        /// <summary>The committed numeric value (the X component of a vector2 entry).</summary>
        public double Value => _value;

        /// <summary>The committed Y component of a vector2 entry; null for scalar entries.</summary>
        public double? ValueY => _valueY;

        /// <summary>
        /// The text being edited for the value. Assignments are parsed per
        /// <see cref="IsInteger"/>; text that fails to parse reverts to the
        /// last valid representation. Vector2 entries are read-only.
        /// </summary>
        public string ValueText
        {
            get => _valueText;
            set => _owner.CommitValue(this, value);
        }

        /// <summary>
        /// Index of the entry's type in the list's picker: 0 = float, 1 = int,
        /// 2 = vector2 (locked entries only). Assignments are validated by the
        /// owning list; rejected assignments snap back.
        /// </summary>
        public int TypeIndex
        {
            get => _typeIndex;
            set => _owner.SetTypeIndex(this, value);
        }

        /// <summary>Whether the value is authored as an integer instead of a float.</summary>
        public bool IsInteger
        {
            get => _typeIndex == 1;
            set => _owner.SetTypeIndex(this, value ? 1 : 0);
        }

        /// <summary>Whether the entry is a vector2 (locked self/player positions).</summary>
        public bool IsVector2 => _typeIndex == 2;

        /// <summary>True while the item sits within the list's locked prefix: fixed position, no rename, no delete, no type change.</summary>
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

        internal void SetVectorInternal(double x, double y, string valueText)
        {
            _value = x;
            _valueY = y;
            SetField(ref _valueText, valueText);
        }

        internal void SetTypeIndexInternal(int typeIndex)
        {
            SetField(ref _typeIndex, typeIndex);
            // IsInteger/IsVector2 are computed from the index; re-notify the bindings.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInteger)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVector2)));
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

        /// <summary>Re-raises <see cref="TypeIndex"/> so rejected type picks snap back.</summary>
        internal void NotifyTypeReset()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TypeIndex)));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string caller = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
