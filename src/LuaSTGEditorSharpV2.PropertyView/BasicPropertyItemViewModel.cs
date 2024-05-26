using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class BasicPropertyItemViewModel(NodeData nodeData, LocalServiceParam localServiceParam) 
        : PropertyItemViewModelBase(nodeData, localServiceParam)
    {
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
    }
}
