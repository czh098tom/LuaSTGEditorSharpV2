using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using System.Numerics;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Preview semantics of the built-in variable nodes: the menu-inserted
    /// 无限 node (int, preview value adjusted per node instance) and the
    /// drag-created self → 自身坐标 / player → 玩家坐标 nodes (vector2).
    /// Unlocked entries keep generating the generic PatternVariable nodes.
    /// </summary>
    public class BuiltinVariableNodeTests
    {
        private static Parameter RootParameterWithVectors()
        {
            var list = new VariableListViewModel();
            var parameter = new Parameter();
            foreach (var (name, value) in list.ToFloats())
            {
                parameter.Floats[name] = value;
            }
            foreach (var (name, value) in list.ToVectors())
            {
                parameter.Vectors[name] = value;
            }
            return parameter;
        }

        private static T Latest<T>(IObservable<T>? source)
        {
            Assert.NotNull(source);
            T latest = default!;
            using var subscription = source!.Subscribe(v => latest = v);
            return latest;
        }

        [Fact]
        public void InfiniteNode_PreviewUsesItsOwnEditorValue()
        {
            var node = new InfiniteNode();
            node.PreviewValueEditor.RawValue = 3;
            var value = Latest(node.OutputValue.Value);

            Assert.Equal(3, value(RootParameterWithVectors()));
            Assert.Equal(3, value(new Parameter()));
        }

        [Fact]
        public void InfiniteNode_DefaultsToTen()
        {
            var node = new InfiniteNode();
            var value = Latest(node.OutputValue.Value);

            Assert.Equal(10, value(new Parameter()));
        }

        [Fact]
        public void InfiniteNode_EachInstanceTunesItsOwnPreviewValue()
        {
            var first = new InfiniteNode();
            var second = new InfiniteNode();
            first.PreviewValueEditor.RawValue = 3;
            second.PreviewValueEditor.RawValue = 25;

            var firstValue = Latest(first.OutputValue.Value);
            var secondValue = Latest(second.OutputValue.Value);

            Assert.Equal(3, firstValue(new Parameter()));
            Assert.Equal(25, secondValue(new Parameter()));
        }

        [Fact]
        public void SelfPositionNode_ReadsVectorScope()
        {
            var node = new SelfPositionNode();
            var position = Latest(node.OutputPosition.Value);

            Assert.Equal(new Vector2(0f, 120f), position(RootParameterWithVectors()));
        }

        [Fact]
        public void PlayerPositionNode_ReadsVectorScope()
        {
            var node = new PlayerPositionNode();
            var position = Latest(node.OutputPosition.Value);

            Assert.Equal(new Vector2(0f, -180f), position(RootParameterWithVectors()));
        }

        [Fact]
        public void PositionNodes_FallBackToZero_WhenScopeEmpty()
        {
            var self = Latest(new SelfPositionNode().OutputPosition.Value);
            var player = Latest(new PlayerPositionNode().OutputPosition.Value);

            Assert.Equal(Vector2.Zero, self(new Parameter()));
            Assert.Equal(Vector2.Zero, player(new Parameter()));
        }

        [Fact]
        public void VectorsPropagate_ThroughPatternParameterCopies()
        {
            // Patterns copy the parameter per element; the seeded vector
            // positions must survive those copies.
            var player = Latest(new PlayerPositionNode().OutputPosition.Value);
            var element = new Parameter(RootParameterWithVectors());

            Assert.Equal(new Vector2(0f, -180f), player(element));
        }

        [Fact]
        public void DropDispatch_LockedBuiltIns_CreateDedicatedNodeTypes()
        {
            var viewModel = new MainViewModel();
            var list = viewModel.VariableList;

            viewModel.AddNodeForVariable(list.Items[0], new System.Windows.Point(0, 0));
            viewModel.AddNodeForVariable(list.Items[1], new System.Windows.Point(0, 0));

            var added = viewModel.Network.Nodes.Items
                .OfType<LinqSTGNodeViewModel>()
                .Skip(1) // the default Shoot node
                .ToArray();
            Assert.IsType<SelfPositionNode>(added[0]);
            Assert.IsType<PlayerPositionNode>(added[1]);
        }

        [Fact]
        public void DropDispatch_UnlockedEntries_CreateGenericNodesBoundToName()
        {
            var viewModel = new MainViewModel();
            var entry = viewModel.VariableList.AddItem();
            entry.Name = "speed";

            viewModel.AddNodeForVariable(entry, new System.Windows.Point(0, 0));

            var added = viewModel.Network.Nodes.Items.OfType<PatternVariableFloatNode>().SingleOrDefault();
            Assert.NotNull(added);
            Assert.Equal("speed", added.NameEditor.RawValue);
        }

        [Fact]
        public void InfiniteNode_IsListedInCreationMenu_AndSerializesItsEditorValue()
        {
            var viewModel = new MainViewModel();
            var node = new InfiniteNode();
            node.PreviewValueEditor.RawValue = 3;
            node.Position = new System.Windows.Point(5, 5);
            viewModel.Network.Nodes.Add(node);

            viewModel.Save();
            Assert.NotNull(viewModel.NetworkJson);

            // Round-trip: the node type comes back with its per-node preview value.
            var reopened = new MainViewModel { NetworkJson = viewModel.NetworkJson };
            reopened.Load();
            var restored = reopened.Network.Nodes.Items.OfType<InfiniteNode>().SingleOrDefault();
            Assert.NotNull(restored);
            Assert.Equal(3, restored.PreviewValueEditor.RawValue);
        }
    }
}
