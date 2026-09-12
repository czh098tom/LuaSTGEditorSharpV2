using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LinqSTG.Expression.ToLua.Serialization;
using Newtonsoft.Json;
using System.Numerics;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Semantics of the blueprint window's variable list: the locked built-in
    /// prefix (self/player), locking, unique names with forced _{n} renames,
    /// add/remove/move, value commits and the locked-only vector2 type.
    /// _infinite is not a list entry (it is a menu-inserted node).
    /// </summary>
    public class VariableListTests
    {
        private static VariableItemViewModel ItemAt(VariableListViewModel list, int index) => list.Items[index];

        [Fact]
        public void DefaultList_HasLockedSelfPlayerPrefix()
        {
            var list = new VariableListViewModel();

            Assert.Equal(2, list.Items.Count);
            Assert.Equal(2, list.LockedCount);

            var self = list.Items[0];
            Assert.Equal("self", self.Name);
            Assert.True(self.IsVector2);
            Assert.True(self.IsLocked);

            var player = list.Items[1];
            Assert.Equal("player", player.Name);
            Assert.True(player.IsVector2);
            Assert.True(player.IsLocked);

            Assert.DoesNotContain(list.Items, i => i.Name == VariableListViewModel.InfiniteName);
        }

        [Fact]
        public void VectorBuiltIns_CarryPreviewPositions()
        {
            var list = new VariableListViewModel();

            var vectors = list.ToVectors();

            Assert.Equal(new Vector2(0f, 120f), vectors["self"]);
            Assert.Equal(new Vector2(0f, -180f), vectors["player"]);
            // Scalar entries only: the built-in vector2s stay out of the float scope.
            Assert.Empty(list.ToFloats());
        }

        [Fact]
        public void Add_CreatesUnlockedFloatItemWithGeneratedName()
        {
            var list = new VariableListViewModel();
            var added = list.AddItem();

            Assert.Equal("_1", added.Name);
            Assert.False(added.IsInteger);
            Assert.False(added.IsVector2);
            Assert.Equal(0.0, added.Value);
            Assert.False(added.IsLocked);
            Assert.Equal(3, list.Items.Count);
            Assert.All(list.Items.Take(2), i => Assert.True(i.IsLocked));
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

            list.MoveItem(2, 2);
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
        public void Rename_ReservedBuiltInName_IsForcedToGeneratedName()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            // a already occupies _1, so the forced name is the next free one.
            a.Name = "self";

            Assert.Equal("_2", a.Name);
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

            foreach (var locked in list.Items)
            {
                locked.Name = "other";
                Assert.Contains(locked.Name, new[] { "self", "player" });
            }
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
            Assert.False(list.RemoveItem(ItemAt(list, 1)));
            Assert.Equal(3, list.Items.Count);
            Assert.True(list.RemoveItem(a));
            Assert.Equal(2, list.Items.Count);
        }

        [Fact]
        public void Move_ReordersEntries_ButNotIntoLockedPrefix()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            var b = list.AddItem();
            a.Name = "a";
            b.Name = "b";

            // Insertion index 4: drop below b moves a to the end.
            list.MoveItem(2, 4);
            Assert.Equal(["self", "player", "b", "a"], list.Items.Select(i => i.Name).ToArray());

            // Insertion index 2: drop above b moves a back, below the locked prefix.
            list.MoveItem(3, 2);
            Assert.Equal(["self", "player", "a", "b"], list.Items.Select(i => i.Name).ToArray());

            // Insertion index 0 is clamped below the locked prefix: b lands
            // right below player, above a.
            list.MoveItem(3, 0);
            Assert.Equal(["self", "player", "b", "a"], list.Items.Select(i => i.Name).ToArray());

            // Locked entries cannot move at all.
            list.MoveItem(0, 2);
            Assert.Equal(["self", "player", "b", "a"], list.Items.Select(i => i.Name).ToArray());
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
        public void CommitValue_Vector2Entry_IsReadOnly()
        {
            var list = new VariableListViewModel();
            var self = ItemAt(list, 0);

            self.ValueText = "5, 5";

            Assert.Equal(0.0, self.Value);
            Assert.Equal("0, 120", self.ValueText);
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
        public void SetTypeIndex_LockedEntry_IsRejected()
        {
            var list = new VariableListViewModel();
            var self = ItemAt(list, 0);
            var player = ItemAt(list, 1);

            self.TypeIndex = 0;
            Assert.Equal(2, self.TypeIndex);
            Assert.True(self.IsVector2);

            player.TypeIndex = 1;
            Assert.Equal(2, player.TypeIndex);
            Assert.True(player.IsVector2);
        }

        [Fact]
        public void SetTypeIndex_Vector2_UnlockedEntry_IsRejected()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();

            a.TypeIndex = 2;

            Assert.Equal(0, a.TypeIndex);
            Assert.False(a.IsVector2);
        }

        [Fact]
        public void SetTypeIndex_UnlockedEntry_SwitchesIntAndFloat()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            a.ValueText = "2.5";

            a.TypeIndex = 1;
            Assert.True(a.IsInteger);
            Assert.Equal(3.0, a.Value);
            Assert.Equal("3", a.ValueText);

            a.TypeIndex = 0;
            Assert.False(a.IsInteger);
            Assert.Equal(3.0, a.Value);
            Assert.Equal("3", a.ValueText);
        }

        [Fact]
        public void ToFloats_SnapshotsScalarNameValueBindings()
        {
            var list = new VariableListViewModel();
            var a = list.AddItem();
            a.Name = "speed";
            a.ValueText = "3.5";

            var floats = list.ToFloats();

            Assert.Equal(3.5f, floats["speed"]);
            Assert.Single(floats);
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
                list.Items.Select(i => (i.Name, i.Value, i.IsInteger, i.IsVector2, i.IsLocked)).ToArray(),
                restored.Items.Select(i => (i.Name, i.Value, i.IsInteger, i.IsVector2, i.IsLocked)).ToArray());
            // The vector2 built-ins keep their components through serialization.
            Assert.Equal(list.ToVectors(), restored.ToVectors());
        }

        [Fact]
        public void LoadFrom_Null_KeepsLockedPrefix()
        {
            var list = new VariableListViewModel();
            list.AddItem();

            list.LoadFrom(null);

            Assert.Equal(2, list.Items.Count);
            Assert.Equal(["self", "player"], list.Items.Select(i => i.Name).ToArray());
            Assert.All(list.Items, i => Assert.True(i.IsLocked));
        }

        [Fact]
        public void LoadFrom_MissingBuiltIns_SeedsLockedPrefix()
        {
            var list = new VariableListViewModel();

            list.LoadFrom([new VariableItemModel("speed", 2.5, false)]);

            Assert.Equal(3, list.Items.Count);
            Assert.Equal("self", list.Items[0].Name);
            Assert.Equal("player", list.Items[1].Name);
            Assert.Equal("speed", list.Items[2].Name);
            Assert.False(list.Items[2].IsLocked);
        }

        [Fact]
        public void LoadFrom_DocumentBuiltIns_KeepFixedIdentityButRestoreValues()
        {
            var list = new VariableListViewModel();

            // Document: proper vector2 entries for self and player, a duplicate
            // unlocked name, and a legacy _infinite entry.
            list.LoadFrom([
                new VariableItemModel("_infinite", 999, true),
                new VariableItemModel("speed", 1, false),
                new VariableItemModel("self", -5, false, ValueY: 200),
                new VariableItemModel("speed", 2, false),
                new VariableItemModel("player", 7, false, ValueY: -90),
            ]);

            // _infinite is dropped: the loop bound moved onto the Infinite node.
            Assert.Equal(["self", "player", "speed", "_1"], list.Items.Select(i => i.Name).ToArray());
            // Identity stays fixed (locked prefix, built-in types)...
            Assert.True(list.Items[0].IsLocked);
            Assert.True(list.Items[0].IsVector2);
            Assert.True(list.Items[1].IsVector2);
            // ...while the values round-trip from the document.
            Assert.Equal(new Vector2(-5f, 200f), list.ToVectors()["self"]);
            Assert.Equal(new Vector2(7f, -90f), list.ToVectors()["player"]);
            // The duplicate 'speed' was uniquified instead of dropped.
            Assert.Equal(1.0, list.Items[2].Value);
        }

        [Fact]
        public void LoadFrom_ScalarBuiltInEntry_FallsBackToDefaultVector()
        {
            var list = new VariableListViewModel();

            // A malformed scalar 'self' entry (no valueY) cannot restore a
            // position: the built-in preview position is used instead.
            list.LoadFrom([new VariableItemModel("self", 5, false)]);

            Assert.Equal(new Vector2(0f, 120f), list.ToVectors()["self"]);
        }

        [Fact]
        public void Values_RoundTrip_ThroughModel_ForAllItems()
        {
            var list = new VariableListViewModel();
            var speed = list.AddItem();
            speed.Name = "speed";
            speed.ValueText = "2.5";

            var restored = new VariableListViewModel();
            restored.LoadFrom(list.ToModel());

            Assert.Equal(2.5, restored.Items.First(i => i.Name == "speed").Value);
            Assert.Equal(list.ToVectors(), restored.ToVectors());
        }

        [Fact]
        public void WindowJson_RoundTrips_AllItemValues()
        {
            var viewModel = new MainViewModel();
            var speed = viewModel.VariableList.AddItem();
            speed.Name = "speed";
            speed.ValueText = "2.5";

            viewModel.Save();
            Assert.NotNull(viewModel.NetworkJson);

            // Every list entry — locked built-ins included — carries its value in
            // the JSON the window hands back to the document; _infinite is not a
            // list entry and never appears in variables.
            var model = JsonConvert.DeserializeObject<NetworkModel>(viewModel.NetworkJson!)!;
            var variables = model.Variables!.ToDictionary(v => v.Name, v => v, StringComparer.Ordinal);
            Assert.Equal(0, variables["self"].Value);
            Assert.Equal(120, variables["self"].ValueY);
            Assert.Equal(-180, variables["player"].ValueY);
            Assert.Equal(2.5, variables["speed"].Value);
            Assert.False(variables.ContainsKey(VariableListViewModel.InfiniteName));

            // And the values survive reopening the window.
            var reopened = new MainViewModel { NetworkJson = viewModel.NetworkJson };
            reopened.Load();
            Assert.Equal(new Vector2(0f, 120f), reopened.VariableList.ToVectors()["self"]);
            Assert.Equal(2.5, reopened.VariableList.Items.First(i => i.Name == "speed").Value);
        }
    }
}
