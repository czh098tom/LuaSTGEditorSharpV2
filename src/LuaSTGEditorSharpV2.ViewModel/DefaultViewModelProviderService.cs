using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DefaultViewModelProviderService : ViewModelProviderServiceBase
    {
        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            viewModel.Text = $"[{dataSource.TypeUID}] (Unknown)";
        }
    }
}
