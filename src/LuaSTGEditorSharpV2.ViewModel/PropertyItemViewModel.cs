using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class PropertyItemViewModel : BaseViewModel
    {
        private string _name;
        private string _value;
        private string _type;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }

        public PropertyItemViewModel(string name, string value, string type = "")
        {
            _name = name;
            _value = value;
            _type = type;
        }
    }
}
