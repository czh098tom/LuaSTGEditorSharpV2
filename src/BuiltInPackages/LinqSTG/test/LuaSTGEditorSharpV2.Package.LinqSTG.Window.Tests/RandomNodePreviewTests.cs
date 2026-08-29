using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math;
using System;
using System.Linq;
using System.Reflection;
using System.Reactive.Linq;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// Preview semantics of the random generator nodes, following
    /// DemoScript.TestRandom: a script-level randomizer is created from the
    /// configured seed at every pattern materialization and attached to the
    /// root <see cref="Parameter.Randomizer"/>; random nodes draw from it while
    /// the pattern is enumerated (once per bullet), and the movement closures
    /// only read the fixed values — dragging the progress slider runs Predict
    /// only and never re-draws. The same seed therefore replays the same values.
    /// </summary>
    public class RandomNodePreviewTests
    {
        private static T Latest<T>(LinqSTGNodeOutputViewModel<T> output)
            => output.Value!.First();

        private static Parameter Seeded(int seed) => new() { Randomizer = new Random(seed) };

        [Fact]
        public void RandomFloat_AdvancesWithinARandomizerAndReplaysWithSameSeed()
        {
            var node = new RandomFloatNode();
            node.InputStartEditor.RawValue = 0f;
            node.InputEndEditor.RawValue = 100f;
            var contextual = Latest(node.OutputValue);

            // One materialization: one randomizer shared by every bullet,
            // advancing per draw.
            var dict = Seeded(42);
            var first = contextual(dict);
            var second = contextual(dict);
            Assert.InRange(first, 0f, 100f);
            Assert.NotEqual(first, second);

            // A new materialization with the same seed replays the sequence.
            var replay = Seeded(42);
            Assert.Equal(first, contextual(replay));
            Assert.Equal(second, contextual(replay));
        }

        [Fact]
        public void RandomInt_SamplesInclusiveRange()
        {
            var node = new RandomIntNode();
            node.InputStartEditor.RawValue = 1;
            node.InputEndEditor.RawValue = 6;
            var contextual = Latest(node.OutputValue);

            var values = Enumerable.Range(0, 200).Select(_ => contextual(Seeded(123))).ToArray();

            Assert.All(values, v => Assert.InRange(v, 1, 6));
        }

        [Fact]
        public void RandomInt_AdvancesWithinASharedRandomizer()
        {
            var node = new RandomIntNode();
            node.InputStartEditor.RawValue = 1;
            node.InputEndEditor.RawValue = 6;
            var contextual = Latest(node.OutputValue);

            var dict = Seeded(123);
            var values = Enumerable.Range(0, 200).Select(_ => contextual(dict)).ToArray();

            Assert.All(values, v => Assert.InRange(v, 1, 6));
            Assert.Equal(1, values.Min());
            Assert.Equal(6, values.Max());
        }

        [Fact]
        public void RandomSign_ReturnsMinusOneOrOne()
        {
            var node = new RandomSignNode();
            var contextual = Latest(node.OutputValue);

            var dict = Seeded(5);
            var values = Enumerable.Range(0, 100).Select(_ => contextual(dict)).ToArray();

            Assert.All(values, v => Assert.Contains(v, new[] { -1, 1 }));
            Assert.Contains(-1, values);
            Assert.Contains(1, values);
        }

        [Fact]
        public void PatternPredictions_AreMaterializedPerUpdatePattern()
        {
            // Mirrors the demo host's GeneratePredictions: pointPredictions must be
            // a materialized collection, not a lazy query — otherwise every
            // UpdatePrediction (each progress-drag frame) re-enumerates the whole
            // pattern and re-draws every random value.
            var viewModel = new MainViewModel();
            var field = typeof(MainViewModel).GetField("pointPredictions",
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var value = field.GetValue(viewModel);

            Assert.NotNull(value);
            Assert.True(value.GetType().IsArray,
                $"pointPredictions should be materialized, got {value.GetType()}");
        }

        [Fact]
        public void Seed_RoundTripsThroughNetworkJson()
        {
            var viewModel = new MainViewModel();
            viewModel.Seed = 42;

            viewModel.Save();

            Assert.NotNull(viewModel.NetworkJson);
            Assert.Contains("\"seed\":42", viewModel.NetworkJson);

            var reloaded = new MainViewModel { NetworkJson = viewModel.NetworkJson };
            reloaded.Load();
            Assert.Equal(42, reloaded.Seed);
        }
    }
}
