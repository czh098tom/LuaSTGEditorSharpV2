using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel
{
    // TODO: Let configuables to decide which update method should be used.
    /// <summary>
    /// Provide functionality of presenting and preserving tree structure on GUI according to <see cref="NodeData"/>.
    /// </summary>
    public abstract class ViewModelProviderServiceBase
        : CompactNodeService<ViewModelProviderServiceProvider, ViewModelProviderServiceBase, NodeViewModelContext, ViewModelProviderServiceSettings>
    {
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
        internal protected virtual void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource
            , NodeViewModelContext context)
        { }
    }
}
