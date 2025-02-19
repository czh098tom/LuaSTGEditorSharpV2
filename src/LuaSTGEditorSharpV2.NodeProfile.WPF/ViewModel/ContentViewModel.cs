using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.NodeProfile.WPF.ViewModel
{
    public class ContentViewModel(string description) : ViewModelBase
    {
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
        }
        private string _description = description;
    }
}
