using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DefaultViewModelProviderService : ViewModelProviderServiceBase
    {
        private static readonly string unknownFormatKey = "unknown_format";

        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            viewModel.Text = string.Format(HostedApplicationHelper.GetService<LocalizationService>()
                .GetString(unknownFormatKey, typeof(DefaultViewModelProviderService).Assembly), dataSource.TypeUID);
        }
    }
}
