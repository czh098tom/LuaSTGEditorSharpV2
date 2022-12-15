using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    public class DocumentViewModel : BaseViewModel
    {
        public ObservableCollection<NodeViewModel> Tree { get; private set; } = new();
    }
}
