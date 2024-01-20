using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.ViewModel
{
    public class SettingsPageViewModel : ViewModelBase
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

        private ObservableCollection<object> _pageItems = [];
        public ObservableCollection<object> PageItems
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
