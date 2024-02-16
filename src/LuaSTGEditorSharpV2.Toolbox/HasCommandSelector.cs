using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Toolbox.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.Toolbox
{
    public class HasCommandSelector : ResourceDictKeySelector<ToolboxItemViewModel>
    {
        public override string CreateKey(ToolboxItemViewModel vm)
        {
            return vm.HasCommand.ToString();
        }

        public override bool HasKeyFromSource(ToolboxItemViewModel vm)
        {
            return true;
        }
    }
}
