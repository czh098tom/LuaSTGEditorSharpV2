using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.ViewModel.Configurable;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    // TODO: Let configuables to decide which update method should be used.
    /// <summary>
    /// Provide functionality of presenting and preserving tree structure on GUI according to <see cref="NodeData"/>.
    /// </summary>
    [ServiceName("ViewModel"), ServiceShortName("vm")]
    public class ViewModelProviderServiceBase 
        : NodeService<ViewModelProviderServiceBase, NodeViewModelContext, ViewModelProviderServiceSettings>
    {
        private static readonly DefaultViewModelProviderService _defaultService = new();

        private static Dictionary<NodeData, NodeViewModel> _mapping = new();

        static ViewModelProviderServiceBase()
        {
            _defaultServiceGetter = () => _defaultService;
        }

        /// <summary>
        /// Unload the internal mapping from <see cref="NodeData"/> to <see cref="NodeViewModel"/> 
        /// to allow garbage collecting on <see cref="NodeData"/> and <see cref="NodeViewModel"/>
        /// </summary>
        /// <param name="target"> the <see cref="NodeData"/> to unload. </param>
        /// <remarks>
        /// Often be called by various commands that creates or deletes nodes.
        /// </remarks>
        public static void UnloadNodeViewModelData(NodeData target)
        {
            _mapping.Remove(target);
            foreach (var child in target.PhysicalChildren)
            {
                UnloadNodeViewModelData(child);
            }
        }

        public static void UpdateViewModelDataRecursive(NodeData dataSource, LocalServiceParam param)
            => UpdateViewModelDataRecursive(dataSource, param, ServiceSettings);

        /// <summary>
        /// Update the <see cref="NodeViewModel"/> attached to the given <see cref="NodeData"/> and its physical child.
        /// </summary>
        /// <param name="dataSource"> the <see cref="NodeData"/> that contains data to update. </param>
        /// <param name="param"> The local params for executing the service. </param>
        /// <param name="serviceSettings"> The <see cref="ViewModelProviderServiceSettings"/> of this action. </param>
        /// <remarks>
        /// Often be called by various commands that manipulates.
        /// TODO: decide update method by <see cref="NodeData"/>.
        /// </remarks>
        public static void UpdateViewModelDataRecursive(NodeData dataSource, LocalServiceParam param
            , ViewModelProviderServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(dataSource, param, serviceSettings);
            UpdateViewModelDataRecursive(dataSource, ctx);
        }

        /// <summary>
        /// Update the <see cref="NodeViewModel"/> attached to the given <see cref="NodeData"/> and its physical child.
        /// </summary>
        /// <param name="dataSource"> the <see cref="NodeData"/> that contains data to update. </param>
        /// <param name="context"> The <see cref="NodeViewModelContext"/> of the node. </param>
        /// <remarks>
        /// Often be called by various commands that manipulates.
        /// TODO: decide update method by <see cref="NodeData"/>.
        /// </remarks>
        private static void UpdateViewModelDataRecursive(NodeData dataSource, NodeViewModelContext context)
        {
            GetServiceOfNode(dataSource).UpdateViewModelData(_mapping[dataSource], dataSource, context);
            context.Push(dataSource);
            foreach (var child in dataSource.PhysicalChildren)
            {
                UpdateViewModelDataRecursive(child, context);
            }
            context.Pop();
        }

        public static void InsertNodeAt(NodeData parent, int position, NodeData child, LocalServiceParam param)
            => InsertNodeAt(parent, position, child, param, ServiceSettings);

        /// <summary>
        /// Insert a child node to a given index at a parent, then do the same to <see cref="NodeViewModel"/>.
        /// </summary>
        /// <param name="parent"> The parent <see cref="NodeData"/>. </param>
        /// <param name="position"> The position among children in parent <see cref="NodeData"/> after inserting. </param>
        /// <param name="child"> The child <see cref="NodeData"/>. </param>
        /// <param name="param"> The local params for executing the service. </param>
        /// <param name="serviceSettings"> The <see cref="ViewModelProviderServiceSettings"/> the settings for this action. </param>
        public static void InsertNodeAt(NodeData parent, int position, NodeData child, LocalServiceParam param
            , ViewModelProviderServiceSettings serviceSettings)
        {
            parent.Insert(position, child);
            var context = GetContextOfNode(child, param, serviceSettings);
            _mapping[parent].Children.Insert(position, CreateViewModelRecursive(child, context));
        }

        public static NodeViewModel CreateViewModelRecursive(NodeData target, LocalServiceParam param)
            => CreateViewModelRecursive(target, param, ServiceSettings);

        /// <summary>
        /// Create <see cref="NodeViewModel"/> recursively for the given <see cref="NodeData"/>.
        /// </summary>
        /// <param name="target"> The target <see cref="NodeData"/>. </param>
        /// <param name="param"> The local params for executing the service. </param>
        /// <param name="serviceSettings"> The <see cref="ViewModelProviderServiceSettings"/> for this action. </param>
        /// <returns> <see cref="NodeViewModel"/> generated. </returns>
        public static NodeViewModel CreateViewModelRecursive(NodeData target, LocalServiceParam param
            , ViewModelProviderServiceSettings serviceSettings)
        {
            return CreateViewModelRecursive(target, GetContextOfNode(target, param, serviceSettings));
        }

        /// <summary>
        /// Create <see cref="NodeViewModel"/> recursively for the given <see cref="NodeData"/>.
        /// </summary>
        /// <param name="target"> The target <see cref="NodeData"/>. </param>
        /// <param name="context"> The <see cref="NodeViewModelContext"/> of the node. </param>
        /// <returns> <see cref="NodeViewModel"/> generated. </returns>
        private static NodeViewModel CreateViewModelRecursive(NodeData target, NodeViewModelContext context)
        {
            NodeViewModel viewModel = CreateViewModel(target, context);
            context.Push(target);
            for (int i = 0; i < target.PhysicalChildren.Count; i++)
            {
                viewModel.Children.Add(CreateViewModelRecursive(target.PhysicalChildren[i], context));
            }
            context.Pop();
            return viewModel;
        }

        /// <summary>
        /// Create <see cref="NodeViewModel"/> for the given <see cref="NodeData"/>.
        /// </summary>
        /// <param name="node"> The target <see cref="NodeData"/>. </param>
        /// <param name="context"> The <see cref="NodeViewModelContext"/> of the node. </param>
        /// <returns> <see cref="NodeViewModel"/> generated. </returns>
        private static NodeViewModel CreateViewModel(NodeData node, NodeViewModelContext context)
        {
            var viewModel = new NodeViewModel(node);
            GetServiceOfNode(node).UpdateViewModelData(viewModel, node, context);
            _mapping.Add(node, viewModel);
            return viewModel;
        }

        /// <summary>
        /// Remove a <see cref="NodeData"/> at given place from parent, then do the same to <see cref="NodeViewModel"/>
        /// </summary>
        /// <param name="parent"> The parent <see cref="NodeData"/>. </param>
        /// <param name="position"> The position among children in parent <see cref="NodeData"/> before removing. </param>
        public static void RemoveNodeAt(NodeData parent, int position)
        {
            _mapping[parent].Children.RemoveAt(position);
            UnloadNodeViewModelData(parent.PhysicalChildren[position]);
            parent.Remove(position);
        }

        public override sealed NodeViewModelContext GetEmptyContext(LocalServiceParam localSettings
            , ViewModelProviderServiceSettings serviceSettings)
        {
            return new NodeViewModelContext(localSettings, serviceSettings);
        }

        /// <summary>
        /// Update the <see cref="NodeViewModel"/> by contents in <see cref="NodeData"/> with the same TypeUID.
        /// </summary>
        /// <param name="viewModel"> The <see cref="NodeViewModel"/> to be updated. </param>
        /// <param name="dataSource"> The data source with the same TypeUID. </param>
        /// <param name="context"> The <see cref="NodeViewModelContext"/> of the node. </param>
        protected virtual void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource
            , NodeViewModelContext context) { }
    }
}
