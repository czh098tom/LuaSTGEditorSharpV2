using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ViewModel.Specialized
{
    [Serializable]
    public class Code : ViewModelProviderServiceBase
    {
        [JsonProperty] public string Icon { get; private set; } = "";

        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            var array = dataSource.GetProperty("code").Split('\n');
            if (array.Length > 1)
            {
                viewModel.Text = $"{array[0]} ...";
            }
            else
            {
                viewModel.Text = array[0];
            }
            viewModel.Icon = Icon;
        }
    }
}
