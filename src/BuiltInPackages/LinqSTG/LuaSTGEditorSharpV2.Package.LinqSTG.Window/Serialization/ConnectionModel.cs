using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using Newtonsoft.Json;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Serialization
{
    public record class ConnectionModel
        (
            [property:JsonProperty("source_node_index", Required = Required.Always)] int SourceNodeIndex,
            [property:JsonProperty("source_port_name", Required = Required.Always)] string SourcePortName,
            [property:JsonProperty("target_node_index", Required = Required.Always)] int TargetNodeIndex,
            [property:JsonProperty("target_port_name", Required = Required.Always)] string TargetPortName
        )
    {
        public static ConnectionModel FromViewModel(ConnectionViewModel viewModel, List<LinqSTGNodeViewModel> nodes)
        {
            if (viewModel is null)
            {
                throw new ArgumentNullException(nameof(viewModel), "ConnectionViewModel cannot be null.");
            }

            var outPort = viewModel.Output;
            var inPort = viewModel.Input;

            var outNode = outPort.Parent as LinqSTGNodeViewModel 
                ?? throw new InvalidOperationException("Output port's parent node is not a LinqSTGNodeViewModel.");
            var inNode = inPort.Parent as LinqSTGNodeViewModel
                ?? throw new InvalidOperationException("Input port's parent node is not a LinqSTGNodeViewModel.");

            var outNodeIndex = nodes.IndexOf(outNode);
            if (outNodeIndex < 0)
            {
                throw new InvalidOperationException("Output node not found in the provided nodes list.");
            }
            var inNodeIndex = nodes.IndexOf(inNode);
            if (inNodeIndex < 0)
            {
                throw new InvalidOperationException("Input node not found in the provided nodes list.");
            }

            var outPortName = outNode.OutputDict.FirstOrDefault(kv => kv.Value == outPort).Key
                ?? throw new InvalidOperationException("Output port does not have a valid name.");

            var inPortName = inNode.InputDict.FirstOrDefault(kv => kv.Value == inPort).Key
                ?? throw new InvalidOperationException("Input port does not have a valid name.");

            return new ConnectionModel(outNodeIndex, outPortName, inNodeIndex, inPortName);
        }
    }
}
