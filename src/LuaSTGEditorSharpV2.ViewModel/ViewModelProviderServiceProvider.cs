using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Editor.Extension;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel
{
    [PackedServiceProvider]
    [ServiceName("ViewModel"), ServiceShortName("vm")]
    public class ViewModelProviderServiceProvider
        : ContextualNodeServiceProvider<ViewModelProviderServiceBase, NodeViewModelContext, ViewModelProviderServiceSettings>
    {
        protected override ViewModelProviderServiceBase DefaultService => _defaultService;

        private readonly ViewModelProviderServiceBase _defaultService;
        private readonly EditorNodeFactory editorNodeFactory;

        public ViewModelProviderServiceProvider(IServiceProvider serviceProvider, EditorNodeFactory editorNodeFactory) : base(serviceProvider)
        {
            _defaultService = new DefaultViewModelProviderService(this, serviceProvider);
            this.editorNodeFactory = editorNodeFactory;
        }

        public override sealed NodeViewModelContext GetEmptyContext(LocalServiceParam localSettings
            , ViewModelProviderServiceSettings serviceSettings)
        {
            return new NodeViewModelContext(ServiceProvider, localSettings, serviceSettings);
        }

        public void UpdateViewModelDataRecursive(NodeViewModel dataSource, LocalServiceParam param)
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
        private void UpdateViewModelDataRecursive(NodeViewModel dataSource, LocalServiceParam param
            , ViewModelProviderServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(dataSource.Source, param, serviceSettings);
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
        private void UpdateViewModelDataRecursive(NodeViewModel dataSource, NodeViewModelContext context)
        {
            GetServiceOfNode(dataSource.Source).UpdateViewModelData(dataSource, dataSource.Source, context);
            using var _ = context.AcquireContextLevelHandle(dataSource.Source);
            foreach (var child in dataSource.Children)
            {
                UpdateViewModelDataRecursive(child, context);
            }
        }
    }
}
