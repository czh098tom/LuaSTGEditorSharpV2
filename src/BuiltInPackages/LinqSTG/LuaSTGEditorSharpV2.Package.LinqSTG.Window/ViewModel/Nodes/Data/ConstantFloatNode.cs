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
    public class ConstantFloatNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel ValueEditor { get; } = new FloatValueEditorViewModel();
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public ConstantFloatNode()
        {
            OutputValue = LinqSTGNodeOutputViewModel.Float(editor: ValueEditor);

            AddOutput("value", OutputValue);
            AddEditor("value", ValueEditor);

            Name = "Float";

            TitleColor = NodeColors.Data;
        }
    }
}
