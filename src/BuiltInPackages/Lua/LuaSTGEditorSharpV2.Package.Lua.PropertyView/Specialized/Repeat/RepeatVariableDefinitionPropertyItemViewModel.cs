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
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat
{
    public class RepeatVariableDefinitionPropertyItemViewModel(ViewModelProviderServiceProvider viewModelProviderServiceProvider,
        RepeatPropertyViewItemTerm term, int index, NodeData nodeData, LocalServiceParam localServiceParam)
        : JsonProxiedPropertyItemViewModel<RepeatVariableDefinition>(nodeData, localServiceParam)
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

        public void SetProxy(string propName, string propInit, string propIncrement)
        {
            var def = new RepeatVariableDefinition(propName, propInit, propIncrement);
            ProxyValue = def;
            _propName = propName;
            _propInit = propInit;
            _propIncrement = propIncrement;
            RaisePropertyChanged(nameof(PropName));
            RaisePropertyChanged(nameof(PropInit));
            RaisePropertyChanged(nameof(PropIncrement));
        }

        public override EditResult ResolveEditingNodeCommand(NodeData nodeData, LocalServiceParam localServiceParam, string edited)
        {
            var commands = new List<CommandBase>();
            if (term.NameRule == null || term.InitRule == null || term.IncrementRule == null) return new EditResult(localServiceParam);
            object idx = index;
            var editName = EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider,
                nodeData, string.Format(term.NameRule.Key, idx), ProxyValue?.Name ?? string.Empty);
            var editValue = EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider,
                nodeData, string.Format(term.InitRule.Key, idx), ProxyValue?.Init ?? string.Empty);
            var editIncrement = EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider,
                nodeData, string.Format(term.IncrementRule.Key, idx), ProxyValue?.Increment ?? string.Empty);
            if (editName != null) commands.Add(editName);
            if (editValue != null) commands.Add(editValue);
            if (editIncrement != null) commands.Add(editIncrement);
            return new EditResult(commands.Count > 0 ? new CompositeCommand(commands) : null, localServiceParam);
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class RepeatVariableDefinitionPropertyItemViewModelFactory(ViewModelProviderServiceProvider viewModelProviderServiceProvider)
    {
        public RepeatVariableDefinitionPropertyItemViewModel Create(RepeatPropertyViewItemTerm term, int index,
            NodeData nodeData, LocalServiceParam localServiceParam)
        {
            return new RepeatVariableDefinitionPropertyItemViewModel(viewModelProviderServiceProvider, term, index, nodeData, localServiceParam);
        }
    }
}
