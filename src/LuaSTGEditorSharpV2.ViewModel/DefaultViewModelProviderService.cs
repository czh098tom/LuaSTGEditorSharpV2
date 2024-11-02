using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DefaultViewModelProviderService(ViewModelProviderServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ViewModelProviderServiceBase(nodeServiceProvider, serviceProvider)
    {
        private static readonly string unknownFormatKey = "unknown_format";

        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            viewModel.Text = context.Format(ServiceProvider.GetRequiredService<LocalizationService>()
                .GetString(unknownFormatKey, typeof(DefaultViewModelProviderService).Assembly), dataSource.TypeUID);
        }
    }
}
