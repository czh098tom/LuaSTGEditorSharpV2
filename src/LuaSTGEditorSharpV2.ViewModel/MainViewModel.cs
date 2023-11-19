using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private PropertyPageViewModel _propertyPage = new();

        public PropertyPageViewModel PropertyPage
        {
            get => _propertyPage;
            set
            {
                _propertyPage = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<DocumentViewModel> Documents { get; private set; } = new();

        public int ActiveDocumentIndex { get; set; } = 0;
    }
}
