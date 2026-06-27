using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement
{
    public class UniformVelocityMovementNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputVelocity { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public UniformVelocityMovementNode()
        {
            InputVelocity = LinqSTGNodeInputViewModel.Vector2("Velocity");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("velocity", InputVelocity);
            AddOutput("movement", OutputMovement);

            Name = "Uniform Velocity Movement";

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputVelocity.ValueChanged.Select(vec 
                => Contextual.Create(dict 
                    => new Parametric<int, Vector2>(t => (vec?.Invoke(dict) ?? Vector2.Zero) * t)));
        }
    }
}
