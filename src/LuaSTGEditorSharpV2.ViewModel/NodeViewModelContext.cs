using LuaSTGEditorSharpV2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class NodeViewModelContext : NodeContextWithSettings<ViewModelProviderServiceSettings>
    {
        public NodeViewModelContext(IServiceProvider serviceProvider, LocalServiceParam localSettings, ViewModelProviderServiceSettings serviceSettings)
            : base(serviceProvider, localSettings, serviceSettings)
        {
        }
    }
}
