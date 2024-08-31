using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Debugging.ViewModel
{
    public class DebugOutputPageViewModel(IServiceProvider serviceProvider) : AnchorableViewModelBase(serviceProvider)
    {
        public override string I18NTitleKey => "panel_output_title";
    }
}
