using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Preview semantics of the variable reference nodes: their typed outputs
    /// resolve the node's name against the parameter's float scope, which the
    /// main view model seeds from the variable list. Int-typed list entries
    /// produce <see cref="PatternVariableIntNode"/>, float-typed entries
    /// <see cref="PatternVariableFloatNode"/>.
    /// </summary>
    public class PatternVariableNodeTests
    {
        private static Parameter ParameterWith(params (string name, float value)[] entries)
        {
            var parameter = new Parameter();
            foreach (var (name, value) in entries)
            {
                parameter.Floats[name] = value;
            }
            return parameter;
        }

        private static Contextual<float> LatestFloat(PatternVariableFloatNode node)
        {
            Assert.NotNull(node.OutputValue.Value);
            Contextual<float> latest = _ => 0f;
            using var subscription = node.OutputValue.Value!.Subscribe(v => latest = v);
            return latest;
        }

        private static Contextual<int> LatestInt(PatternVariableIntNode node)
        {
            Assert.NotNull(node.OutputValue.Value);
            Contextual<int> latest = _ => 0;
            using var subscription = node.OutputValue.Value!.Subscribe(v => latest = v);
            return latest;
        }

        [Fact]
        public void FloatOutput_ReadsNameFromParameterFloats()
        {
            var node = new PatternVariableFloatNode();
            node.NameEditor.RawValue = "speed";
            var value = LatestFloat(node);

            Assert.Equal(3.5f, value(ParameterWith(("speed", 3.5f))));
        }

        [Fact]
        public void FloatOutput_FallsBackToZero_WhenNameMissing()
        {
            var node = new PatternVariableFloatNode();
            node.NameEditor.RawValue = "missing";
            var value = LatestFloat(node);

            Assert.Equal(0f, value(new Parameter()));
        }

        [Fact]
        public void IntOutput_ReadsNameFromParameterFloats()
        {
            var node = new PatternVariableIntNode();
            node.NameEditor.RawValue = "_infinite";
            var value = LatestInt(node);

            Assert.Equal(10, value(ParameterWith(("_infinite", 10f))));
        }

        [Fact]
        public void IntOutput_FallsBackToZero_WhenNameMissing()
        {
            var node = new PatternVariableIntNode();
            node.NameEditor.RawValue = "missing";
            var value = LatestInt(node);

            Assert.Equal(0, value(new Parameter()));
        }

        [Fact]
        public void Outputs_FollowNameEdits()
        {
            var floatNode = new PatternVariableFloatNode();
            floatNode.NameEditor.RawValue = "speed";
            floatNode.NameEditor.RawValue = "angle";

            var intNode = new PatternVariableIntNode();
            intNode.NameEditor.RawValue = "speed";
            intNode.NameEditor.RawValue = "count";

            var parameter = ParameterWith(("speed", 3.5f), ("angle", 90f), ("count", 7f));

            Assert.Equal(90f, LatestFloat(floatNode)(parameter));
            Assert.Equal(7, LatestInt(intNode)(parameter));
        }

        [Fact]
        public void Outputs_SeeOuterScopeEntries_ThroughPatternParameterCopies()
        {
            // Patterns copy the parameter per element (Parameter(Parameter) copy
            // ctor); the seeded variable values must survive those copies.
            var node = new PatternVariableFloatNode();
            node.NameEditor.RawValue = "_infinite";
            var value = LatestFloat(node);

            var root = ParameterWith(("_infinite", 10f));
            var element = new Parameter(root);

            Assert.Equal(10f, value(element));
        }
    }
}
