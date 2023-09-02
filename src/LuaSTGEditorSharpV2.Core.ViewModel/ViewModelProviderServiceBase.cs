using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.ViewModel.Configurable;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    [ServiceName("ViewModel"), ServiceShortName("vm")]
    public class ViewModelProviderServiceBase 
        : NodeService<ViewModelProviderServiceBase, ViewModelContext, ViewModelProviderServiceSettings>
    {
        private static readonly DefaultViewModelProviderService _defaultService = new();

        private static Dictionary<NodeData, NodeViewModel> _mapping = new();

        static ViewModelProviderServiceBase()
        {
            _defaultServiceGetter = () => _defaultService;
        }

        public static void UnloadNodeViewModelData(NodeData target)
        {
            _mapping.Remove(target);
            foreach (var child in target.PhysicalChildren)
            {
                UnloadNodeViewModelData(child);
            }
        }

        public static void UpdateViewModelData(NodeData dataSource)
        {
            GetServiceOfNode(dataSource).UpdateViewModelData(_mapping[dataSource], dataSource);
        }

        private static void UpdateViewModelDataRecursive(NodeData dataSource)
        {
            UpdateViewModelData(dataSource);
            foreach (var child in dataSource.PhysicalChildren)
            {
                UpdateViewModelData(child);
            }
        }

        public static void AddNodeAt(NodeData parent, int position, NodeData source)
        {
            parent.Insert(position, source);
            _mapping[parent].Children.Insert(position, CreateViewModelFor(source));
            CreateViewModelRecursive(_mapping[parent].Children[position].Children, source.PhysicalChildren);
        }

        public static void CreateRoot(IList<NodeViewModel> target, NodeData root)
        {
            NodeViewModel viewModel = CreateViewModelFor(root);
            target.Add(viewModel);
            CreateViewModelRecursive(viewModel.Children, root.PhysicalChildren);
        }

        private static void CreateViewModelRecursive(IList<NodeViewModel> target, IReadOnlyList<NodeData> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                CreateRoot(target, source[i]);
            }
        }

        private static NodeViewModel CreateViewModelFor(NodeData node)
        {
            var viewModel = new NodeViewModel(node);
            GetServiceOfNode(node).UpdateViewModelData(viewModel, node);
            _mapping.Add(node, viewModel);
            return viewModel;
        }

        public static void RemoveNodeAt(NodeData parent, int position)
        {
            _mapping[parent].Children.RemoveAt(position);
            UnloadNodeViewModelData(parent.PhysicalChildren[position]);
            parent.Remove(position);
        }

        public override ViewModelContext GetEmptyContext(LocalSettings localSettings)
        {
            return new ViewModelContext(localSettings);
        }

        protected virtual void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource) { }
    }
}
