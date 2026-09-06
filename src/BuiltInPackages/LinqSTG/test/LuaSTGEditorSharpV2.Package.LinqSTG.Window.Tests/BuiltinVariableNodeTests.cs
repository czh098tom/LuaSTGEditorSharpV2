using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using System.Numerics;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Preview semantics of the locked built-in variable nodes and the drag
    /// dispatch that creates them: _infinite → 无限 (int), self → 自身坐标
    /// (vector2), player → 玩家坐标 (vector2). Unlocked entries keep generating
    /// the generic PatternVariable nodes.
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
        public void InfiniteNode_ReadsListValue()
        {
            var node = new InfiniteNode();
            var value = Latest(node.OutputValue.Value);

            Assert.Equal(10, value(RootParameterWithVectors()));
        }

        [Fact]
        public void InfiniteNode_FallsBackToDefault_WhenScopeEmpty()
        {
            var node = new InfiniteNode();
            var value = Latest(node.OutputValue.Value);

            Assert.Equal(10, value(new Parameter()));
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
            viewModel.AddNodeForVariable(list.Items[2], new System.Windows.Point(0, 0));

            var added = viewModel.Network.Nodes.Items
                .OfType<LinqSTGNodeViewModel>()
                .Skip(1) // the default Shoot node
                .ToArray();
            Assert.IsType<InfiniteNode>(added[0]);
            Assert.IsType<SelfPositionNode>(added[1]);
            Assert.IsType<PlayerPositionNode>(added[2]);
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
    }
}
