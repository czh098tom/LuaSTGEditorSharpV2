using DynamicData;
using LinqSTG;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern;
using NodeNetwork.Toolkit.ValueNode;
using System.Numerics;
using System.Reactive.Linq;
using System.Reflection;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    public class NumericConversionPreviewTests
    {
        [Theory]
        [InlineData(2.5f, 2)]
        [InlineData(3.5f, 4)]
        [InlineData(-2.5f, -2)]
        [InlineData(-3.5f, -4)]
        [InlineData(2147483520f, 2147483520)]
        [InlineData(-2147483648f, int.MinValue)]
        public void IntegerEntryPoints_KeepValidConversion(float input, int expected)
        {
            var parameter = new Parameter();
            parameter.Floats["value"] = input;
            var converter = new FloatToIntNode();
            converter.InputFloat.Editor = new FloatValueEditorViewModel { RawValue = input };
            var variable = new PatternVariableIntNode();
            variable.NameEditor.RawValue = "value";
            var repeater = new RepeaterKey("value", "value").GetRepeater(parameter);

            Assert.Equal(expected, converter.OutputInt.Value!.First()(parameter));
            Assert.Equal(expected, variable.OutputValue.Value!.First()(parameter));
            Assert.Equal(expected, repeater.ID);
            Assert.Equal(expected, repeater.Total);
        }

        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        [InlineData(float.MaxValue)]
        [InlineData(2147483648f)]
        [InlineData(-2147483904f)]
        public void InvalidConversion_ReportsTheOutputAndDoesNotProduceZero(float value)
        {
            var converter = new FloatToIntNode();
            converter.InputFloat.Editor = new FloatValueEditorViewModel { RawValue = value };
            var error = Assert.ThrowsAny<Exception>(() => converter.OutputInt.Value!.First()(new Parameter()));
            Assert.IsType<OverflowException>(error.InnerException);
            var port = Assert.IsType<LinqSTGPortViewModel>(converter.OutputInt.Port);
            Assert.Contains("Int32", port.EvaluationError);
            Assert.NotEqual(port.PortColor, port.DisplayColor);
        }

        [Fact]
        public void PreviewFailure_IsContainedAndRecoversAfterInputCorrection()
        {
            using var viewModel = new MainViewModel();
            var converter = new FloatToIntNode();
            var editor = new FloatValueEditorViewModel { RawValue = float.NaN };
            converter.InputFloat.Editor = editor;
            viewModel.Network.Nodes.Add(converter);
            Contextual<IEnumerable<PointPrediction>> preview = parameter =>
            {
                converter.OutputInt.Value!.First()(parameter);
                return [];
            };
            typeof(MainViewModel).GetField("activePreviewResult", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(viewModel, preview);
            var update = typeof(MainViewModel).GetMethod("UpdatePreviewPattern", BindingFlags.Instance | BindingFlags.NonPublic)!;
            update.Invoke(viewModel, null);
            Assert.NotNull(viewModel.PreviewError);
            Assert.Empty(viewModel.Points);
            var port = Assert.IsType<LinqSTGPortViewModel>(converter.OutputInt.Port);
            Assert.NotNull(port.EvaluationError);

            editor.RawValue = 3;
            update.Invoke(viewModel, null);
            Assert.Null(viewModel.PreviewError);
            Assert.Null(port.EvaluationError);
            Assert.Equal(port.PortColor, port.DisplayColor);
        }

    }
}
