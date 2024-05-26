using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class ItemValueUpdatedEventArgs(int index, PropertyTabViewModel.ItemValueUpdatedEventArgs args) : EventArgs
    {
        public int Index { get; private set; } = index;
        public PropertyTabViewModel.ItemValueUpdatedEventArgs Args { get; private set; } = args;
    }
}
