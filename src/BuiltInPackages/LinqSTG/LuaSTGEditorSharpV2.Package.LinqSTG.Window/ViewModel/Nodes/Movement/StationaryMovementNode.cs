using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement
{
    public class StationaryMovementNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputPosition { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public StationaryMovementNode()
        {
            InputPosition = LinqSTGNodeInputViewModel.Vector2("Position");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("position", InputPosition);
            AddOutput("movement", OutputMovement);

            Name = "Stationary Movement";
            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputPosition.ValueChanged
                .Select(vec
                    => Contextual.Create(dict
                        => new Parametric<int, Vector2>(t => vec?.Invoke(dict) ?? Vector2.Zero)));
        }
    }
}
