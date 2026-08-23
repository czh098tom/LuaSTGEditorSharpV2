using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_float", Order = 0)]
    public class ConstantFloatNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel ValueEditor { get; } = new FloatValueEditorViewModel();
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public ConstantFloatNode()
        {
            OutputValue = LinqSTGNodeOutputViewModel.Float(editor: ValueEditor);

            AddOutput("value", OutputValue);
            AddEditor("value", ValueEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_float;

            TitleColor = NodeColors.Data;
        }
    }
}
