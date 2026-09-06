using LinqSTG.Expression.ToLua.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    /// <summary>
    /// The variable list shown along the left edge of the blueprint area.
    /// Each entry binds a unique name to a numeric value (integer or float).
    /// The entries act as outer-scope variables: the preview seeds them into the
    /// root pattern parameter, and translation assumes they exist outside the
    /// generated code.
    /// The first <see cref="LockedCount"/> entries are locked: fixed position,
    /// not deletable, not renamable, type not changeable (but still draggable
    /// into the blueprint area, where each generates its dedicated node type).
    /// The locked prefix is fixed to three built-ins:
    ///   - <see cref="InfiniteName"/> (_infinite = integer 10, the Shoot loop bound)
    ///   - <see cref="SelfName"/> (self, vector2: the shooter's own position)
    ///   - <see cref="PlayerName"/> (player, vector2: the player's position)
    /// </summary>
    public class VariableListViewModel : INotifyPropertyChanged
    {
        /// <summary>The name of the fixed first entry used by the generated Shoot loop.</summary>
        public const string InfiniteName = "_infinite";

        public const double InfiniteDefaultValue = 10.0;

        /// <summary>The locked self entry: the shooter's own position (vector2).</summary>
        public const string SelfName = "self";

        /// <summary>The locked player entry: the player's position (vector2).</summary>
        public const string PlayerName = "player";

        /// <summary>Preview positions of the vector2 built-ins: the shooter sits in the upper half of the play field, the player near the bottom.</summary>
        public const double SelfDefaultX = 0.0, SelfDefaultY = 120.0;
        public const double PlayerDefaultX = 0.0, PlayerDefaultY = -180.0;

        /// <summary>Index of the vector2 entry in the list's type picker.</summary>
        public const int Vector2TypeIndex = 2;

        private int _lockedCount = 3;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raised after any committed change (add/remove/move/rename/value/type).</summary>
        public event EventHandler? Changed;

        public ObservableCollection<VariableItemViewModel> Items { get; } = [];

        /// <summary>Number of locked entries at the head of the list (the built-in prefix).</summary>
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
            AddLockedDefault(InfiniteName, isInteger: true, InfiniteDefaultValue, y: null);
            AddLockedDefault(SelfName, isInteger: false, SelfDefaultX, SelfDefaultY);
            AddLockedDefault(PlayerName, isInteger: false, PlayerDefaultX, PlayerDefaultY);
            RefreshLocks();
        }

        public VariableItemViewModel AddItem()
        {
            var item = new VariableItemViewModel(this);
            item.SetNameInternal(NextFreeGeneratedName());
            item.SetValueInternal(0, FormatValue(0, isInteger: false));
            item.SetTypeIndexInternal(0);
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

        /// <summary>Parses and commits the value text; unparseable text reverts to the last valid value. Vector2 entries are read-only.</summary>
        public void CommitValue(VariableItemViewModel item, string? text)
        {
            if (!Items.Contains(item)) return;
            if (item.IsVector2)
            {
                item.NotifyValueReset();
                return;
            }

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

        /// <summary>
        /// Commits a type pick: locked entries keep their type, and vector2 is
        /// reserved for the locked built-ins; rejected picks snap back. Switching
        /// between int and float normalizes the stored value.
        /// </summary>
        public void SetTypeIndex(VariableItemViewModel item, int typeIndex)
        {
            if (!Items.Contains(item)) return;

            var index = Items.IndexOf(item);
            if (index < _lockedCount || typeIndex == Vector2TypeIndex)
            {
                item.NotifyTypeReset();
                return;
            }

            if (item.TypeIndex == typeIndex)
            {
                item.NotifyTypeReset();
                return;
            }

            var isInteger = typeIndex == 1;
            var value = isInteger ? Math.Round(item.Value, MidpointRounding.AwayFromZero) : item.Value;
            item.SetTypeIndexInternal(typeIndex);
            item.SetValueInternal(value, FormatValue(value, isInteger));
            OnChanged();
        }

        /// <summary>Snapshot of the scalar entries as name-to-float bindings, seeded into the root pattern parameter.</summary>
        public Dictionary<string, float> ToFloats()
        {
            var result = new Dictionary<string, float>(Items.Count, StringComparer.Ordinal);
            foreach (var item in Items)
            {
                if (!item.IsVector2)
                {
                    result[item.Name] = (float)item.Value;
                }
            }
            return result;
        }

        /// <summary>Snapshot of the vector2 entries as name-to-position bindings, seeded into the root parameter's vector scope.</summary>
        public Dictionary<string, Vector2> ToVectors()
        {
            var result = new Dictionary<string, Vector2>(StringComparer.Ordinal);
            foreach (var item in Items)
            {
                if (item.IsVector2)
                {
                    result[item.Name] = new Vector2((float)item.Value, (float)(item.ValueY ?? 0));
                }
            }
            return result;
        }

        public VariableItemModel[] ToModel()
        {
            var models = new VariableItemModel[Items.Count];
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                models[i] = item.IsVector2
                    ? new VariableItemModel(item.Name, item.Value, IsInteger: false, ValueY: item.ValueY)
                    : new VariableItemModel(item.Name, item.Value, item.IsInteger);
            }
            return models;
        }

        /// <summary>
        /// Replaces the list with deserialized entries. The locked prefix is
        /// rebuilt from the built-in defaults (its names are reserved); the
        /// remaining document entries are restored as unlocked entries with
        /// defensively de-duplicated names.
        /// </summary>
        public void LoadFrom(VariableItemModel[]? models)
        {
            Items.Clear();

            var usedNames = new HashSet<string>(StringComparer.Ordinal) { InfiniteName, SelfName, PlayerName };
            if (models is { Length: > 0 })
            {
                foreach (var model in models)
                {
                    if (model is null) continue;
                    var candidate = (model.Name ?? string.Empty).Trim();
                    if (candidate.Length == 0)
                    {
                        continue;
                    }
                    if (IsReservedName(candidate))
                    {
                        // Reserved built-in names are re-seeded from the defaults below.
                        continue;
                    }
                    // Duplicates within the document are uniquified defensively.
                    while (usedNames.Contains(candidate))
                    {
                        candidate = NextFreeGeneratedName();
                    }
                    usedNames.Add(candidate);
                    var item = new VariableItemViewModel(this);
                    item.SetNameInternal(candidate);
                    item.SetTypeIndexInternal(model.IsInteger ? 1 : 0);
                    item.SetValueInternal(model.Value, FormatValue(model.Value, model.IsInteger));
                    Items.Add(item);
                }
            }

            EnsureLockedDefaults();
            RefreshLocks();
            OnChanged();
        }

        /// <summary>Whether the name belongs to the locked built-in prefix.</summary>
        public static bool IsReservedName(string name)
        {
            return string.Equals(name, InfiniteName, StringComparison.Ordinal)
                || string.Equals(name, SelfName, StringComparison.Ordinal)
                || string.Equals(name, PlayerName, StringComparison.Ordinal);
        }

        private void EnsureLockedDefaults()
        {
            AddLockedDefault(InfiniteName, isInteger: true, InfiniteDefaultValue, y: null, insertAt: 0);
            AddLockedDefault(SelfName, isInteger: false, SelfDefaultX, SelfDefaultY, insertAt: 1);
            AddLockedDefault(PlayerName, isInteger: false, PlayerDefaultX, PlayerDefaultY, insertAt: 2);
        }

        /// <summary>Creates one built-in locked entry; <paramref name="y"/> non-null marks a vector2 entry.</summary>
        private void AddLockedDefault(string name, bool isInteger, double x, double? y, int? insertAt = null)
        {
            var item = new VariableItemViewModel(this);
            item.SetNameInternal(name);
            if (y is null)
            {
                item.SetTypeIndexInternal(isInteger ? 1 : 0);
                item.SetValueInternal(x, FormatValue(x, isInteger));
            }
            else
            {
                item.SetTypeIndexInternal(Vector2TypeIndex);
                item.SetVectorInternal(x, y.Value, FormatVector(x, y.Value));
            }
            if (insertAt is int index)
            {
                Items.Insert(Math.Min(index, Items.Count), item);
            }
            else
            {
                Items.Add(item);
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

        private string NextFreeGeneratedName()
        {
            for (int n = 1; ; n++)
            {
                var candidate = $"_{n}";
                var taken = Items.Any(item => string.Equals(item.Name, candidate, StringComparison.Ordinal));
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

        private static string FormatVector(double x, double y)
        {
            return string.Create(CultureInfo.CurrentCulture, $"{(float)x}, {(float)y}");
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
