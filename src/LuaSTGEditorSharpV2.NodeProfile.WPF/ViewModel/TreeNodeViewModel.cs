using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.NodeProfile.WPF.ViewModel
{
    public class TreeNodeViewModel : ViewModelBase
    {
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                RaisePropertyChanged();
            }
        }
        private string _icon;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }
        private string _title;

        public ObservableCollection<TreeNodeViewModel> Children { get; } = [];

        public ContentViewModel? Content { get; }

        public TreeNodeViewModel(string icon, string title, ContentViewModel? content = null)
        {
            _icon = icon;
            _title = title;
            Content = content;
        }
    }
}
