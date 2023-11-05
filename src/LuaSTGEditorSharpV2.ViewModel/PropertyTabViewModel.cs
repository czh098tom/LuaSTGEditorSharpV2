using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class PropertyTabViewModel : BaseViewModel
    {
        private string caption = string.Empty;

        public ObservableCollection<PropertyItemViewModel> Properties { get; private set; } = new();

        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                RaisePropertyChanged();
            }
        }
    }
}
