using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DocumentTabViewModel : ViewModelBase
    {
        public NodeViewModel Header { get; private set; }

        public ObservableCollection<NodeViewModel> Tree { get; private set; } = [];

        public DocumentTabViewModel(NodeViewModel nodeViewModel)
        {
            Header = nodeViewModel;
            Tree.Add(nodeViewModel);
        }
    }
}
