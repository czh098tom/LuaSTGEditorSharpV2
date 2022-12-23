using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private DocumentViewModel _document = new();
        private PropertyPageViewModel _propertyPage = new();

        public DocumentViewModel Document
        {
            get => _document;
            set
            {
                _document = value;
                RaisePropertyChanged();
            }
        }

        public PropertyPageViewModel PropertyPage
        {
            get => _propertyPage;
            set
            {
                _propertyPage = value;
                RaisePropertyChanged();
            }
        }
    }
}
