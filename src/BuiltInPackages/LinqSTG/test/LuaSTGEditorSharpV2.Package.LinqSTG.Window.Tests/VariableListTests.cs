using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LinqSTG.Expression.ToLua.Serialization;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Semantics of the blueprint window's variable list: locking, unique names
    /// with forced _{n} renames, add/remove/move and value commits.
    /// </summary>
    public class VariableListTests
    {
        private static VariableItemViewModel ItemAt(VariableListViewModel list, int index) => list.Items[index];

        [Fact]
        public void DefaultList_HasLockedInfiniteIntegerTen()
        {
            var list = new VariableListViewModel();

            Assert.Single(list.Items);
            var first = list.Items[0];
            Assert.Equal("_infinite", first.Name);
            Assert.True(first.IsInteger);
            Assert.Equal(10.0, first.Value);
            Assert.True(first.IsLocked);
        }

        [Fact]
        public void Add_CreatesUnlockedItemWithGeneratedName()
        {
            var list = new VariableListViewModel();
            var added = list.AddItem();

            Assert.Equal("_1", added.Name);
            Assert.False(added.IsInteger);
            Assert.Equal(0.0, added.Value);
            Assert.False(added.IsLocked);
            Assert.Equal(2, list.Items.Count);
            Assert.True(list.Items[0].IsLocked);
        }

        [Fact]
        public void Changed_RaisesOnEveryCommit()
        {
            var list = new VariableListViewModel();
            var count = 0;
            list.Changed += (_, _) => count++;

            var item = list.AddItem();
            Assert.Equal(1, count);

            item.Name = "speed";
            Assert.Equal(2, count);

            item.ValueText = "3.5";
            Assert.Equal(3, count);

            item.IsInteger = true;
            Assert.Equal(4, count);

            list.MoveItem(1, 1);
            Assert.Equal(4, count);

            list.RemoveItem(item);
            Assert.Equal(5, count);
        }

        [Fact]
        public void Rename_Duplicate_IsForcedToGeneratedName()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            a.Name = "speed";
            var b = list.AddItem();
            b.Name = "angle";

            // Duplicating a's name forces b to the next free generated name.
            b.Name = "speed";

            Assert.Equal("_1", b.Name);
            Assert.Equal("speed", a.Name);
        }

        [Fact]
        public void Rename_Empty_IsForcedToGeneratedName()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            a.Name = "speed";

            a.Name = "   ";

            Assert.Equal("_1", a.Name);
        }

        [Fact]
        public void Rename_LockedItem_IsIgnored()
        {
            var list = new VariableListViewModel();
            var locked = ItemAt(list, 0);

            locked.Name = "other";

            Assert.Equal("_infinite", locked.Name);
        }

        [Fact]
        public void Rename_TrimsWhitespace()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();

            a.Name = "  speed  ";

            Assert.Equal("speed", a.Name);
        }

        [Fact]
        public void Remove_LockedItem_Fails_UnlockedSucceeds()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();

            Assert.False(list.RemoveItem(ItemAt(list, 0)));
            Assert.Equal(2, list.Items.Count);
            Assert.True(list.RemoveItem(a));
            Assert.Single(list.Items);
        }

        [Fact]
        public void Move_ReordersEntries_ButNotIntoLockedPrefix()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            var b = list.AddItem();
            a.Name = "a";
            b.Name = "b";

            // Insertion index 3: drop below b moves a to the end.
            list.MoveItem(1, 3);
            Assert.Equal(["_infinite", "b", "a"], list.Items.Select(i => i.Name).ToArray());

            // Insertion index 1: drop above b moves a back, still below _infinite.
            list.MoveItem(2, 1);
            Assert.Equal(["_infinite", "a", "b"], list.Items.Select(i => i.Name).ToArray());

            // Insertion index 0 is clamped below the locked prefix: b lands
            // right below _infinite, above a.
            list.MoveItem(2, 0);
            Assert.Equal(["_infinite", "b", "a"], list.Items.Select(i => i.Name).ToArray());

            // Locked entries cannot move at all.
            list.MoveItem(0, 2);
            Assert.Equal(["_infinite", "b", "a"], list.Items.Select(i => i.Name).ToArray());
        }

        [Fact]
        public void CommitValue_ParsesPerType_AndRevertsInvalidText()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();

            a.ValueText = "2.5";
            Assert.Equal(2.5, a.Value);
            Assert.Equal("2.5", a.ValueText);

            a.ValueText = "not a number";
            Assert.Equal(2.5, a.Value);
            Assert.Equal("2.5", a.ValueText);

            // Integer mode rejects fractions and reverts.
            a.IsInteger = true;
            Assert.Equal("3", a.ValueText);
            a.ValueText = "4";
            Assert.Equal(4.0, a.Value);
            a.ValueText = "4.5";
            Assert.Equal(4.0, a.Value);
            Assert.Equal("4", a.ValueText);
        }

        [Fact]
        public void SetInteger_RoundsValue()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            a.ValueText = "3.7";

            a.IsInteger = true;

            Assert.True(a.IsInteger);
            Assert.Equal(4.0, a.Value);
            Assert.Equal("4", a.ValueText);
        }

        [Fact]
        public void ToFloats_SnapshotsNameValueBindings()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            a.Name = "speed";
            a.ValueText = "3.5";

            var floats = list.ToFloats();

            Assert.Equal(10f, floats["_infinite"]);
            Assert.Equal(3.5f, floats["speed"]);
        }

        [Fact]
        public void Model_RoundTrip_PreservesEntries()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            a.Name = "speed";
            a.ValueText = "3.5";
            var b = list.AddItem();
            b.IsInteger = true;
            b.ValueText = "7";

            var restored = new VariableListViewModel();
            restored.LoadFrom(list.ToModel());

            Assert.Equal(
                list.Items.Select(i => (i.Name, i.Value, i.IsInteger, i.IsLocked)).ToArray(),
                restored.Items.Select(i => (i.Name, i.Value, i.IsInteger, i.IsLocked)).ToArray());
        }

        [Fact]
        public void LoadFrom_Null_KeepsLockedDefault()
        {
            var list = new VariableListViewModel();
            list.AddItem();

            list.LoadFrom(null);

            Assert.Single(list.Items);
            Assert.Equal("_infinite", list.Items[0].Name);
            Assert.True(list.Items[0].IsLocked);
        }

        [Fact]
        public void LoadFrom_MissingInfinite_SeedsItFirst()
        {
            var list = new VariableListViewModel();

            list.LoadFrom([new VariableItemModel("speed", 2.5, false)]);

            Assert.Equal("_infinite", list.Items[0].Name);
            Assert.True(list.Items[0].IsLocked);
            Assert.Equal("speed", list.Items[1].Name);
            Assert.False(list.Items[1].IsLocked);
        }

        [Fact]
        public void LoadFrom_DuplicateNames_AreUniquified()
        {
            var list = new VariableListViewModel();

            list.LoadFrom([
                new VariableItemModel("_infinite", 10, true),
                new VariableItemModel("speed", 1, false),
                new VariableItemModel("speed", 2, false),
                new VariableItemModel("_infinite", 3, false),
            ]);

            Assert.Equal(4, list.Items.Count);
            Assert.Equal(list.Items.Count, list.Items.Select(i => i.Name).ToHashSet().Count);
            Assert.Equal("_infinite", list.Items[0].Name);
        }
    }
}
