using LinqSTG.Expression.ToLua.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// The variable list shown along the left edge of the blueprint area.
    /// Each entry binds a unique name to a numeric value (integer or float).
    /// The entries act as outer-scope variables: the preview seeds them into the
    /// root pattern parameter, and translation assumes they exist outside the
    /// generated code. The first <see cref="LockedCount"/> entries are locked:
    /// fixed position, not deletable, not renamable (but still draggable into
    /// the blueprint area as <see cref="Nodes.Data.PatternVariableNodeBase"/>).
    /// </summary>
    public class VariableListViewModel : INotifyPropertyChanged
    {
        /// <summary>The name of the fixed first entry used by the generated Shoot loop.</summary>
        public const string InfiniteName = "_infinite";

        public const double InfiniteDefaultValue = 10.0;

        private int _lockedCount = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raised after any committed change (add/remove/move/rename/value/type).</summary>
        public event EventHandler? Changed;

        public ObservableCollection<VariableItemViewModel> Items { get; } = [];

        /// <summary>Number of locked entries at the head of the list.</summary>
        public int LockedCount
        {
            get => _lockedCount;
            set
            {
                if (_lockedCount == value) return;
                _lockedCount = value;
                RefreshLocks();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LockedCount)));
            }
        }

        public VariableListViewModel()
        {
            // The fixed first entry: _infinite = integer 10, locked.
            var infinite = new VariableItemViewModel(this);
            infinite.SetNameInternal(InfiniteName);
            infinite.SetIntegerInternal(true);
            infinite.SetValueInternal(InfiniteDefaultValue, FormatValue(InfiniteDefaultValue, isInteger: true));
            Items.Add(infinite);
            RefreshLocks();
        }

        public VariableItemViewModel AddItem()
        {
            var item = new VariableItemViewModel(this);
            item.SetNameInternal(NextFreeGeneratedName());
            item.SetValueInternal(0, FormatValue(0, isInteger: false));
            item.SetIntegerInternal(false);
            Items.Add(item);
            RefreshLocks();
            OnChanged();
            return item;
        }

        public bool RemoveItem(VariableItemViewModel item)
        {
            var index = Items.IndexOf(item);
            if (index < 0 || index < _lockedCount) return false;
            Items.RemoveAt(index);
            RefreshLocks();
            OnChanged();
            return true;
        }

        /// <summary>
        /// Moves an entry so that it lands at the given insertion position of the
        /// pre-move list. Locked entries cannot move, and nothing can be inserted
        /// into the locked prefix.
        /// </summary>
        public void MoveItem(int oldIndex, int newIndex)
        {
            if (oldIndex < _lockedCount || oldIndex >= Items.Count) return;
            var insertion = Math.Clamp(newIndex, _lockedCount, Items.Count);
            var target = oldIndex < insertion ? insertion - 1 : insertion;
            if (target == oldIndex) return;
            Items.Move(oldIndex, target);
            OnChanged();
        }

        /// <summary>
        /// Commits a new name for the entry: locked entries keep their name, and
        /// empty or duplicated names are forced to the next free _{n}.
        /// </summary>
        public void RenameItem(VariableItemViewModel item, string? candidate)
        {
            var index = Items.IndexOf(item);
            if (index < 0) return;

            // Locked entries cannot be renamed; re-notify so the UI snaps back.
            if (index < _lockedCount)
            {
                item.NotifyNameReset();
                return;
            }

            var name = (candidate ?? string.Empty).Trim();
            if (name.Length == 0 || IsNameTaken(name, item))
            {
                name = NextFreeGeneratedName();
            }

            if (string.Equals(item.Name, name, StringComparison.Ordinal))
            {
                item.NotifyNameReset();
                return;
            }

            item.SetNameInternal(name);
            OnChanged();
        }

        /// <summary>Parses and commits the value text; unparseable text reverts to the last valid value.</summary>
        public void CommitValue(VariableItemViewModel item, string? text)
        {
            if (!Items.Contains(item)) return;

            var parsed = (text ?? string.Empty).Trim();
            if (TryParseValue(parsed, item.IsInteger, out var value))
            {
                item.SetValueInternal(value, FormatValue(value, item.IsInteger));
                OnChanged();
            }
            else
            {
                item.NotifyValueReset();
            }
        }

        /// <summary>Switches the value between integer and float authoring, normalizing the stored value.</summary>
        public void SetInteger(VariableItemViewModel item, bool isInteger)
        {
            if (!Items.Contains(item) || item.IsInteger == isInteger) return;

            var value = isInteger ? Math.Round(item.Value, MidpointRounding.AwayFromZero) : item.Value;
            item.SetIntegerInternal(isInteger);
            item.SetValueInternal(value, FormatValue(value, isInteger));
            OnChanged();
        }

        /// <summary>Snapshot of the entries as name-to-float bindings, seeded into the root pattern parameter.</summary>
        public Dictionary<string, float> ToFloats()
        {
            var result = new Dictionary<string, float>(Items.Count, StringComparer.Ordinal);
            foreach (var item in Items)
            {
                result[item.Name] = (float)item.Value;
            }
            return result;
        }

        public VariableItemModel[] ToModel()
        {
            var models = new VariableItemModel[Items.Count];
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                models[i] = new VariableItemModel(item.Name, item.Value, item.IsInteger);
            }
            return models;
        }

        /// <summary>
        /// Replaces the list with deserialized entries. The locked prefix is
        /// restored as-is (or seeded with the defaults when absent/invalid);
        /// names are de-duplicated defensively for hand-edited documents.
        /// </summary>
        public void LoadFrom(VariableItemModel[]? models)
        {
            Items.Clear();

            var usedNames = new HashSet<string>(StringComparer.Ordinal);
            if (models is { Length: > 0 })
            {
                foreach (var model in models)
                {
                    if (model is null) continue;
                    var name = UniqueName((model.Name ?? string.Empty).Trim(), usedNames);
                    usedNames.Add(name);
                    var item = new VariableItemViewModel(this);
                    item.SetNameInternal(name);
                    item.SetIntegerInternal(model.IsInteger);
                    item.SetValueInternal(model.Value, FormatValue(model.Value, model.IsInteger));
                    Items.Add(item);
                }
            }

            EnsureLockedDefaults();
            RefreshLocks();
            OnChanged();
        }

        private void EnsureLockedDefaults()
        {
            // Keep _infinite as the fixed first entry: promote it to the head when the
            // document carries it at another position, or seed it when it is missing.
            var existing = Items.FirstOrDefault(item => string.Equals(item.Name, InfiniteName, StringComparison.Ordinal));
            if (existing is null)
            {
                var item = new VariableItemViewModel(this);
                item.SetNameInternal(InfiniteName);
                item.SetIntegerInternal(true);
                item.SetValueInternal(InfiniteDefaultValue, FormatValue(InfiniteDefaultValue, isInteger: true));
                Items.Insert(0, item);
            }
            else if (Items.IndexOf(existing) != 0)
            {
                Items.Move(Items.IndexOf(existing), 0);
            }
        }

        private void RefreshLocks()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].IsLocked = i < _lockedCount;
            }
        }

        private bool IsNameTaken(string name, VariableItemViewModel except)
        {
            foreach (var item in Items)
            {
                if (!ReferenceEquals(item, except) && string.Equals(item.Name, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private string UniqueName(string candidate, HashSet<string> usedNames)
        {
            if (candidate.Length == 0 || usedNames.Contains(candidate))
            {
                candidate = NextFreeGeneratedName(usedNames);
            }
            return candidate;
        }

        private string NextFreeGeneratedName(HashSet<string>? extraUsed = null)
        {
            for (int n = 1; ; n++)
            {
                var candidate = $"_{n}";
                var taken = extraUsed?.Contains(candidate) == true
                    || Items.Any(item => string.Equals(item.Name, candidate, StringComparison.Ordinal));
                if (!taken) return candidate;
            }
        }

        private static bool TryParseValue(string text, bool isInteger, out double value)
        {
            if (isInteger)
            {
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var i))
                {
                    value = i;
                    return true;
                }
                return Fail(out value);
            }
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var d))
            {
                if (!double.IsFinite(d)) return Fail(out value);
                value = d;
                return true;
            }
            return Fail(out value);

            static bool Fail(out double value)
            {
                value = 0;
                return false;
            }
        }

        private static string FormatValue(double value, bool isInteger)
        {
            if (isInteger)
            {
                return ((long)Math.Round(value, MidpointRounding.AwayFromZero)).ToString(CultureInfo.CurrentCulture);
            }
            return ((float)value).ToString("R", CultureInfo.CurrentCulture);
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
