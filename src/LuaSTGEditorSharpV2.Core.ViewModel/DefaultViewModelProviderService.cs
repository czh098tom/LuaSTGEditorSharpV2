using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    public class DefaultViewModelProviderService : ViewModelProviderServiceBase
    {
        protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource)
        {
            viewModel.Text = $"[{dataSource.TypeUID}] (Unknown)";
        }
    }
}
