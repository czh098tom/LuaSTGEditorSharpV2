using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DocumentViewModel : ViewModelBase
    {
        public ObservableCollection<NodeViewModel> Tree { get; private set; } = new();

        public string Title { get; private set; } = string.Empty;

        public DocumentViewModel(string title)
        {
            Title = title;
        }
    }
}
