using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DocumentViewModel : DockingViewModelBase
    {
        public ObservableCollection<NodeViewModel> Tree { get; private set; } = [];

        private string _title = string.Empty;
        public override string Title => _title;

        public DocumentViewModel(string title)
        {
            _title = title;
        }
    }
}
