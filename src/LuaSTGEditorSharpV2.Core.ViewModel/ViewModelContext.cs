using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    public class ViewModelContext : NodeContext<ViewModelProviderServiceSettings>
    {
        public ViewModelContext(LocalParams localSettings, ViewModelProviderServiceSettings serviceSettings) 
            : base(localSettings, serviceSettings)
        {
        }
    }
}
