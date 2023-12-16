using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized
{
    public class VariableDefinitionPropertyItemViewModel 
        : JsonProxiedPropertyItemViewModel<VariableDefinitionPropertyItemViewModel.VariableDefinition>
    {
        public record class VariableDefinition(
            string Name,
            string Value
            )
        { }

        private string _propName = string.Empty;
        private string _propValue = string.Empty;

        public string PropName
        {
            get => _propName;
            set
            {
                _propName = value;
                ProxyValue = new VariableDefinition(_propName, _propValue);
                RaisePropertyChanged();
            }
        }

        public string PropValue
        {
            get => _propValue;
            set
            {
                _propValue = value;
                ProxyValue = new VariableDefinition(_propName, _propValue);
                RaisePropertyChanged();
            }
        }

        public VariableDefinitionPropertyItemViewModel(string propName, string propValue)
            : base(new VariableDefinition(propName, propValue))
        {
            _propName = propName;
            _propValue = propValue;
        }
    }
}
