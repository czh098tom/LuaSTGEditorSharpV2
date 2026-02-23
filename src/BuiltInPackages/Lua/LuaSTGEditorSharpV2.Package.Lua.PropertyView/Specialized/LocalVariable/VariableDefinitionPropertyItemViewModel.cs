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
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable
{
    public class VariableDefinitionPropertyItemViewModel(EditorNodeFactory factory,
        LocalVariablePropertyViewItemTerm term, int index, EditorNode nodeData, LocalServiceParam localServiceParam,
        PropertyEditWizardProviderService propertyEditWizardProviderService)
        : JsonProxiedPropertyItemViewModel<VariableDefinition>(nodeData, localServiceParam, propertyEditWizardProviderService)
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

        public override EditResult ResolveEditingNodeCommand(EditorNode nodeData, LocalServiceParam localServiceParam, string edited)
        {
            var doc = nodeData.Document;
            var path = nodeData.GetPath();
            if (term.NameRule == null || term.ValueRule == null) return new EditResult(localServiceParam);
            IEnumerable<CommandBase?> Get()
            {
                if (term.NameRule == null || term.ValueRule == null) yield break;
                object idx = index;
                yield return CheckedCommand.Property.Modify(doc, path,
                    string.Format(term.NameRule.Key, idx), ProxyValue?.Name ?? string.Empty);
                yield return CheckedCommand.Property.Modify(doc, path,
                    string.Format(term.ValueRule.Key, idx), ProxyValue?.Value ?? string.Empty);
            }
            return new EditResult(Commands.FromEnumerable(Get()), false, localServiceParam);
        }

        protected override void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e)
        {
            if (term.NameRule == null || term.ValueRule == null) return;
            if (e.Key == string.Format(term.NameRule.Key, index))
            {
                SetProxy(e.NewValue, _propValue);
            }
            else if (e.Key == string.Format(term.ValueRule.Key, index))
            {
                SetProxy(_propName, e.NewValue); 
            }
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class VariableDefinitionPropertyItemViewModelFactory(EditorNodeFactory factory,
        PropertyEditWizardProviderService propertyEditWizardProviderService)
    {
        public VariableDefinitionPropertyItemViewModel Create(LocalVariablePropertyViewItemTerm term, int index,
            EditorNode nodeData, LocalServiceParam localServiceParam)
        {
            return new VariableDefinitionPropertyItemViewModel(factory, term, index, nodeData, localServiceParam, propertyEditWizardProviderService);
        }
    }
}
