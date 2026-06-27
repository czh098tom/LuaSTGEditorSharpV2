using DynamicData;
using LinqSTG;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    public class TakeVariableFromContextNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputValue { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public TakeVariableFromContextNode()
        {
            InputValue = LinqSTGNodeInputViewModel.String("Key");
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");

            AddInput("key", InputValue);
            AddOutput("value", OutputValue);

            Name = "Take Variable";

            TitleColor = NodeColors.Operator;

            OutputValue.Value = InputValue.ValueChanged
                .Select(s => Contextual.Create(dict => dict.GetValueOrDefault(s?.Invoke(dict) ?? string.Empty, 0f)));
        }
    }
}
