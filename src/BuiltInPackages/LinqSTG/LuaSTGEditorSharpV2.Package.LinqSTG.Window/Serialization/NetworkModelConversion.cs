using DynamicData;
using LinqSTG;
using LinqSTG.Expression.ToLua.Serialization;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using Newtonsoft.Json.Linq;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Serialization
{
    public static class NetworkModelConversion
    {
        private static readonly Dictionary<string, Type> _nodeTypeRegistry = BuildNodeTypeRegistry();

        public static NetworkModel FromViewModel(NetworkViewModel network)
        {
            if (network is null)
            {
                throw new ArgumentNullException(nameof(network), "NetworkViewModel cannot be null.");
            }
            var nodeList = network.Nodes.Items.OfType<LinqSTGNodeViewModel>().ToList();
            var connectionList = network.Connections.Items.ToList();

            var nodes = new NodeModel[network.Nodes.Count];
            for (int i = 0; i < nodeList.Count; i++)
            {
                nodes[i] = FromNodeViewModel(nodeList[i]);
            }
            var connections = new ConnectionModel[network.Connections.Count];
            for (int i = 0; i < connectionList.Count; i++)
            {
                connections[i] = FromConnectionViewModel(connectionList[i], nodeList);
            }
            return new(nodes, connections);
        }

        public static void ApplyToNetwork(this NetworkModel model, NetworkViewModel network)
        {
            if (network is null)
            {
                throw new ArgumentNullException(nameof(network), "NetworkViewModel cannot be null.");
            }
            var nodes = new LinqSTGNodeViewModel[model.Nodes.Length];
            network.Connections.Clear();
            network.Nodes.Clear();
            for (int i = 0; i < model.Nodes.Length; i++)
            {
                var vm = CreateNodeViewModel(model.Nodes[i]);
                network.Nodes.Add(vm);
                nodes[i] = vm;
            }

            for (int i = 0; i < model.Connections.Length; i++)
            {
                var connectionModel = model.Connections[i]
                    ?? throw new InvalidOperationException($"ConnectionModel at index {i} is null.");

                var sourceNode = nodes[connectionModel.SourceNodeIndex];
                var targetNode = nodes[connectionModel.TargetNodeIndex];
                if (sourceNode is null || targetNode is null)
                {
                    throw new InvalidOperationException($"Source or target node not found for connection at index {i}.");
                }
                var sourcePort = sourceNode.OutputDict[connectionModel.SourcePortName];
                var targetPort = targetNode.InputDict[connectionModel.TargetPortName];
                network.Connections.Add(new LinqSTGConnectionViewModel(network, targetPort, sourcePort));
            }
        }

        private static NodeModel FromNodeViewModel(LinqSTGNodeViewModel viewModel)
        {
            var x = viewModel.Position.X;
            var y = viewModel.Position.Y;
            var editors = new JObject();

            foreach (var editor in viewModel.EditorDict)
            {
                if (editor.Value is IContextualValueEditorViewModel<float> floatEditor)
                {
                    editors[editor.Key] = JToken.FromObject(floatEditor.RawValue);
                }
                else if (editor.Value is IContextualValueEditorViewModel<int> intEditor)
                {
                    editors[editor.Key] = JToken.FromObject(intEditor.RawValue);
                }
                else if (editor.Value is IContextualValueEditorViewModel<string> stringEditor)
                {
                    editors[editor.Key] = JToken.FromObject(stringEditor.RawValue);
                }
                else if (editor.Value is IContextualValueEditorViewModel<IntervalType> intervalEditor)
                {
                    editors[editor.Key] = JToken.FromObject(intervalEditor.RawValue);
                }
                else if (editor.Value is IContextualValueEditorViewModel<BulletShape> shapeEditor)
                {
                    editors[editor.Key] = JToken.FromObject(shapeEditor.RawValue);
                }
            }

            return new(viewModel.NodeType, x, y, editors);
        }

        private static LinqSTGNodeViewModel CreateNodeViewModel(NodeModel model)
        {
            if (!_nodeTypeRegistry.TryGetValue(model.NodeType, out var type))
            {
                throw new InvalidOperationException($"Unknown node type '{model.NodeType}'.");
            }
            var viewModel = (LinqSTGNodeViewModel?)Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Could not create instance of type '{type.AssemblyQualifiedName}'.");
            viewModel.Position = new(model.X, model.Y);

            foreach (var editor in model.Editors)
            {
                if (viewModel.EditorDict.TryGetValue(editor.Key, out var editorViewModel))
                {
                    if (editorViewModel is IContextualValueEditorViewModel<float> floatContextual)
                    {
                        floatContextual.RawValue = editor.Value?.ToObject<float>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to float.");
                    }
                    else if (editorViewModel is IContextualValueEditorViewModel<int> intContextual)
                    {
                        intContextual.RawValue = editor.Value?.ToObject<int>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to int.");
                    }
                    else if (editorViewModel is IContextualValueEditorViewModel<string> stringContextual)
                    {
                        stringContextual.RawValue = editor.Value?.ToObject<string>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to string.");
                    }
                    else if (editorViewModel is IContextualValueEditorViewModel<IntervalType> intervalContextual)
                    {
                        intervalContextual.RawValue = editor.Value?.ToObject<IntervalType>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to IntervalType.");
                    }
                    else if (editorViewModel is IContextualValueEditorViewModel<BulletShape> shapeContextual)
                    {
                        shapeContextual.RawValue = editor.Value?.ToObject<BulletShape>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to BulletShape.");
                    }
                }
            }

            return viewModel;
        }

        private static ConnectionModel FromConnectionViewModel(ConnectionViewModel viewModel, List<LinqSTGNodeViewModel> nodes)
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

            return new(outNodeIndex, outPortName, inNodeIndex, inPortName);
        }

        private static Dictionary<string, Type> BuildNodeTypeRegistry()
        {
            var registry = new Dictionary<string, Type>(StringComparer.Ordinal);
            var baseType = typeof(LinqSTGNodeViewModel);
            var assembly = baseType.Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract && baseType.IsAssignableFrom(type))
                {
                    var name = type.Name;
                    var nodeType = name.EndsWith("Node", StringComparison.Ordinal) ? name[..^4] : name;
                    registry[nodeType] = type;
                }
            }
            return registry;
        }
    }
}
