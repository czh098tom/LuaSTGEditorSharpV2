using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    public class PreviewNode : LinqSTGNodeViewModel
    {
        public const string ShapeEditorKey = "shape";
        public const string DiameterEditorKey = "diameter";
        private const float DefaultDiameter = 8f;

        public BulletShapeEditorViewModel ShapeEditor { get; } = new();
        public FloatValueEditorViewModel DiameterEditor { get; } = new();

        public LinqSTGNodeInputViewModel<Contextual<BulletShape>?> InputShape { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputDiameter { get; }
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }

        public IObservable<Contextual<IEnumerable<PointPrediction>>> Result { get; }

        public override IObservable<Contextual<IEnumerable<PointPrediction>>> PreviewResult => Result;

        public PreviewNode()
        {
            DiameterEditor.RawValue = DefaultDiameter;

            InputShape = new LinqSTGNodeInputViewModel<Contextual<BulletShape>?>
            {
                Name = "Shape",
                Editor = ShapeEditor,
                Port = null
            };
            InputDiameter = new LinqSTGNodeInputViewModel<Contextual<float>?>
            {
                Name = "Diameter",
                Editor = DiameterEditor,
                Port = null
            };
            InputPattern = LinqSTGNodeInputViewModel.Pattern("Pattern");
            InputMovement = LinqSTGNodeInputViewModel.Movement("Movement");

            AddInput(ShapeEditorKey, InputShape);
            AddInput(DiameterEditorKey, InputDiameter);
            AddInput("pattern", InputPattern);
            AddInput("movement", InputMovement);
            AddEditor(ShapeEditorKey, ShapeEditor);
            AddEditor(DiameterEditorKey, DiameterEditor);

            Name = "Preview";
            TitleColor = NodeColors.Shoot;

            Result = InputPattern.ValueChanged
                .CombineLatest(
                    InputMovement.ValueChanged,
                    InputShape.ValueChanged,
                    InputDiameter.ValueChanged,
                    (pattern, movement, shape, diameter)
                        => Contextual.Create(dict => new PointShooter<Parameter>(
                            dict => movement?.Invoke(dict ?? Parameter.Empty),
                            shape?.Invoke(dict) ?? BulletShape.Circle,
                            diameter?.Invoke(dict) ?? DefaultDiameter)
                            .Shoot(pattern?.Invoke(dict) ?? global::LinqSTG.Pattern.Empty<Parameter, int>())));
        }
    }
}
