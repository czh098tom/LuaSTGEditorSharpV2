using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using log4net.Repository.Hierarchy;
using Microsoft.VisualBasic.ApplicationServices;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Transformation
{
    public class AssignNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputValueEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputTransformation { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputValue { get; }
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputKey { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Parameter>> OutputTransformation { get; }

        public AssignNode()
        {
            InputTransformation = LinqSTGNodeInputViewModel.Transformation("Transformation");
            InputValue = LinqSTGNodeInputViewModel.Float("Value", InputValueEditor);
            InputKey = LinqSTGNodeInputViewModel.String("Key");
            OutputTransformation = LinqSTGNodeOutputViewModel.Transformation("Transformation");

            AddInput("transformation", InputTransformation);
            AddInput("value", InputValue);
            AddInput("key", InputKey);
            AddOutput("transformation", OutputTransformation);

            Name = "Assign";

            TitleColor = NodeColors.Transformation;

            OutputTransformation.Value = InputTransformation.ValueChanged
                .CombineLatest(InputKey.ValueChanged, InputValue.ValueChanged,
                    (trans, key, value) => Contextual.Create(dict =>
                    {
                        dict = trans?.Invoke(dict) ?? dict;

                        var parameter = new Parameter(dict);
                        var inputKey = key?.Invoke(dict) ?? "Key";
                        var inputValue = value?.Invoke(dict) ?? 0;

                        parameter[inputKey] = inputValue;
                        return parameter;
                    }));
        }
    }
}
