using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    public class ShootNode : LinqSTGNodeViewModel
    {
        public const string ShooterNameEditorKey = "shooter_name";
        public const string ShapeEditorKey = "shape";
        public const string DiameterEditorKey = "diameter";
        private const float DefaultDiameter = 8f;

        public StringValueEditorViewModel ShooterNameEditor { get; } = new();
        public BulletShapeEditorViewModel ShapeEditor { get; } = new();
        public FloatValueEditorViewModel DiameterEditor { get; } = new();

        public LinqSTGNodeInputViewModel<Contextual<string>?> InputShooterName { get; }
        public LinqSTGNodeInputViewModel<Contextual<BulletShape>?> InputShape { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputDiameter { get; }
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputMovement { get; }

        public IObservable<Contextual<IEnumerable<PointPrediction>>> Result { get; }

        public ShootNode()
        {
            DiameterEditor.RawValue = DefaultDiameter;

            InputShooterName = new LinqSTGNodeInputViewModel<Contextual<string>?>
            {
                Name = "Shooter Name",
                Editor = ShooterNameEditor,
                Port = null
            };
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

            AddInput(ShooterNameEditorKey, InputShooterName);
            AddInput(ShapeEditorKey, InputShape);
            AddInput(DiameterEditorKey, InputDiameter);
            AddInput("pattern", InputPattern);
            AddInput("movement", InputMovement);
            AddEditor(ShooterNameEditorKey, ShooterNameEditor);
            AddEditor(ShapeEditorKey, ShapeEditor);
            AddEditor(DiameterEditorKey, DiameterEditor);

            Name = "Shoot";

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
