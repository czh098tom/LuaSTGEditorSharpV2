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
    [NodeCreationMenu("Movement", TitleKey = "linqstg_window_node_uniformVelocityMovement")]
    public class UniformVelocityMovementNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputVelocity { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public UniformVelocityMovementNode()
        {
            InputVelocity = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_velocity);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("velocity", InputVelocity);
            AddOutput("movement", OutputMovement);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_uniformVelocityMovement;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputVelocity.ValueChanged.Select(vec 
                => Contextual.Create(dict 
                    => new Parametric<float, Vector2>(t => (vec?.Invoke(dict) ?? Vector2.Zero) * t)));
        }
    }
}
