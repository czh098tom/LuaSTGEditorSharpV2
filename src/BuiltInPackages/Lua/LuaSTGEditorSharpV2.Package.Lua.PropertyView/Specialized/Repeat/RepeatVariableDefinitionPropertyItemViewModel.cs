using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat
{
    public class RepeatVariableDefinitionPropertyItemViewModel
        : JsonProxiedPropertyItemViewModel<RepeatVariableDefinition>
    {
        private string _propName = string.Empty;
        private string _propInit = string.Empty;
        private string _propIncrement = string.Empty;

        public string PropName
        {
            get => _propName;
            set
            {
                _propName = value;
                ProxyValue = new RepeatVariableDefinition(_propName, _propInit, _propIncrement);
                RaisePropertyChanged();
            }
        }

        public string PropInit
        {
            get => _propInit;
            set
            {
                _propInit = value;
                ProxyValue = new RepeatVariableDefinition(_propName, _propInit, _propIncrement);
                RaisePropertyChanged();
            }
        }

        public string PropIncrement
        {
            get => _propIncrement;
            set
            {
                _propIncrement = value;
                ProxyValue = new RepeatVariableDefinition(_propName, _propInit, _propIncrement);
                RaisePropertyChanged();
            }
        }

        public RepeatVariableDefinitionPropertyItemViewModel(string propName, string propValue, string propIncrement,
            NodeData nodeData, LocalServiceParam localServiceParam)
            : base(new RepeatVariableDefinition(propName, propValue, propIncrement), nodeData, localServiceParam)
        {
            _propName = propName;
            _propInit = propValue;
            _propIncrement = propIncrement;
        }
    }
}
