using System;
using System.Numerics;
using System.Reactive.Linq;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// The locked <c>player</c> list entry: the player's position as a
    /// vector2. The preview reads the position from the root parameter's vector
    /// scope (seeded from the variable list); translation emits
    /// <c>player.x</c>/<c>player.y</c>.
    /// Created by dragging the locked list entry into the blueprint area.
    /// </summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_playerPosition", Order = 9)]
    public class PlayerPositionNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputPosition { get; }

        public PlayerPositionNode()
        {
            OutputPosition = LinqSTGNodeOutputViewModel.Vector2(
                global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_position);

            AddOutput("position", OutputPosition);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_playerPosition;

            TitleColor = NodeColors.Data;

            OutputPosition.Value = Observable.Return(Contextual.Create<Vector2>(dict =>
                dict.Vectors.GetValueOrDefault(VariableListViewModel.PlayerName, Vector2.Zero)));
        }
    }
}
