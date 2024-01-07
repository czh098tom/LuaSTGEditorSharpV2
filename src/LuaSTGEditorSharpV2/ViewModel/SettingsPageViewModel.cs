using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }

        private object _pageItems = new();
        public object PageItems
        {
            get => _pageItems;
            set
            {
                _pageItems = value;
                RaisePropertyChanged();
            }
        }
    }
}
