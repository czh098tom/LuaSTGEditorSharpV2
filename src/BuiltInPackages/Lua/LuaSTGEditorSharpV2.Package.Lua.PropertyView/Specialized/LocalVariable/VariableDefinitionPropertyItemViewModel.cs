using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable
{
    public class VariableDefinitionPropertyItemViewModel(ViewModelProviderServiceProvider viewModelProviderServiceProvider,
        LocalVariablePropertyViewItemTerm term, int index, NodeData nodeData, LocalServiceParam localServiceParam)
        : JsonProxiedPropertyItemViewModel<VariableDefinition>(nodeData, localServiceParam)
    {
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

        public void SetProxy(string propName, string propValue)
        {
            var def = new VariableDefinition(propName, propValue);
            ProxyValue = def;
            _propName = propName;
            _propValue = propValue;
            RaisePropertyChanged(nameof(PropName));
            RaisePropertyChanged(nameof(PropValue));
        }

        public override CommandBase? ResolveEditingNodeCommand(NodeData nodeData, LocalServiceParam context, string edited)
        {
            var commands = new List<CommandBase>();
            if (term.NameRule == null || term.ValueRule == null) return null;
            object idx = index;
            var editName = EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider, 
                nodeData, string.Format(term.NameRule.Key, idx), ProxyValue?.Name ?? string.Empty);
            var editValue = EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider, 
                nodeData, string.Format(term.ValueRule.Key, idx), ProxyValue?.Value ?? string.Empty);
            if (editName != null) commands.Add(editName);
            if (editValue != null) commands.Add(editValue);
            return commands.Count > 0 ? new CompositeCommand(commands) : null;
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class VariableDefinitionPropertyItemViewModelFactory(ViewModelProviderServiceProvider viewModelProviderServiceProvider)
    {
        public VariableDefinitionPropertyItemViewModel Create(LocalVariablePropertyViewItemTerm term, int index,
            NodeData nodeData, LocalServiceParam localServiceParam)
        {
            return new VariableDefinitionPropertyItemViewModel(viewModelProviderServiceProvider, term, index, nodeData, localServiceParam);
        }
    }
}
