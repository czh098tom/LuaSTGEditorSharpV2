using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class RibbonViewModel : ViewModelBase
    {
        public InsertPanelViewModel InsertPanel { get; } = new();

        public QueuedBoolHandle IsEnabledHandle { get; private set; }
        public bool IsEnabled
        {
            get => IsEnabledHandle.Value;
        }

        public RibbonViewModel()
        {
            IsEnabledHandle = new(this, nameof(IsEnabled));
        }
    }
}
