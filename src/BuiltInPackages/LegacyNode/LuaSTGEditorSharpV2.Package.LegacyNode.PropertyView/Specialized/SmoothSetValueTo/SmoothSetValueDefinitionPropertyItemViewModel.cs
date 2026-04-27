using System;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.PropertyView.Specialized.SmoothSetValueTo
{
    public class SmoothSetValueDefinitionPropertyItemViewModel(
        SmoothSetValuePropertyViewItemTerm term, int index, EditorNode nodeData, LocalServiceParam localServiceParam,
        PropertyEditWizardProviderService propertyEditWizardProviderService)
        : JsonProxiedPropertyItemViewModel<SmoothSetValueDefinition>(nodeData, localServiceParam, propertyEditWizardProviderService)
    {
        private string _variableName = string.Empty;
        private string _targetValue = string.Empty;
        private string _interpolationMode = string.Empty;
        private string _modificationMode = string.Empty;

        public string VariableName
        {
            get => _variableName;
            set
            {
                _variableName = value;
                ProxyValue = new SmoothSetValueDefinition(_variableName, _targetValue, _interpolationMode, _modificationMode);
                RaisePropertyChanged();
            }
        }

        public string TargetValue
        {
            get => _targetValue;
            set
            {
                _targetValue = value;
                ProxyValue = new SmoothSetValueDefinition(_variableName, _targetValue, _interpolationMode, _modificationMode);
                RaisePropertyChanged();
            }
        }

        public string InterpolationMode
        {
            get => _interpolationMode;
            set
            {
                _interpolationMode = value;
                ProxyValue = new SmoothSetValueDefinition(_variableName, _targetValue, _interpolationMode, _modificationMode);
                RaisePropertyChanged();
            }
        }

        public string ModificationMode
        {
            get => _modificationMode;
            set
            {
                _modificationMode = value;
                ProxyValue = new SmoothSetValueDefinition(_variableName, _targetValue, _interpolationMode, _modificationMode);
                RaisePropertyChanged();
            }
        }

        public void SetProxy(string variableName, string targetValue, string interpolationMode, string modificationMode)
        {
            var def = new SmoothSetValueDefinition(variableName, targetValue, interpolationMode, modificationMode);
            ProxyValue = def;
            _variableName = variableName;
            _targetValue = targetValue;
            _interpolationMode = interpolationMode;
            _modificationMode = modificationMode;
            RaisePropertyChanged(nameof(VariableName));
            RaisePropertyChanged(nameof(TargetValue));
            RaisePropertyChanged(nameof(InterpolationMode));
            RaisePropertyChanged(nameof(ModificationMode));
        }

        public override EditResult ResolveEditingNodeCommand(EditorNode nodeData, LocalServiceParam localServiceParam, string edited)
        {
            var doc = nodeData.Document;
            var path = nodeData.GetPath();
            if (term.VariableNameRule == null || term.TargetValueRule == null || term.InterpolationModeRule == null || term.ModificationModeRule == null)
            {
                return new EditResult(localServiceParam);
            }
            IEnumerable<CommandBase?> Get()
            {
                if (term.VariableNameRule == null || term.TargetValueRule == null || term.InterpolationModeRule == null || term.ModificationModeRule == null) yield break;
                object idx = index;
                yield return CheckedCommand.Property.Modify(doc, path,
                    string.Format(term.VariableNameRule.Key, idx), ProxyValue?.VariableName ?? string.Empty);
                yield return CheckedCommand.Property.Modify(doc, path,
                    string.Format(term.TargetValueRule.Key, idx), ProxyValue?.TargetValue ?? string.Empty);
                yield return CheckedCommand.Property.Modify(doc, path,
                    string.Format(term.InterpolationModeRule.Key, idx), ProxyValue?.InterpolationMode ?? string.Empty);
                yield return CheckedCommand.Property.Modify(doc, path,
                    string.Format(term.ModificationModeRule.Key, idx), ProxyValue?.ModificationMode ?? string.Empty);
            }
            return new EditResult(Commands.FromEnumerable(Get()), false, localServiceParam);
        }

        protected override void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e)
        {
            if (term.VariableNameRule == null || term.TargetValueRule == null || term.InterpolationModeRule == null || term.ModificationModeRule == null) return;
            if (e.Key == string.Format(term.VariableNameRule.Key, index))
            {
                _variableName = e.NewValue;
                RaisePropertyChanged(nameof(VariableName));
            }
            else if (e.Key == string.Format(term.TargetValueRule.Key, index))
            {
                _targetValue = e.NewValue;
                RaisePropertyChanged(nameof(TargetValue));
            }
            else if (e.Key == string.Format(term.InterpolationModeRule.Key, index))
            {
                _interpolationMode = e.NewValue;
                RaisePropertyChanged(nameof(InterpolationMode));
            }
            else if (e.Key == string.Format(term.ModificationModeRule.Key, index))
            {
                _modificationMode = e.NewValue;
                RaisePropertyChanged(nameof(ModificationMode));
            }
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class SmoothSetValueDefinitionPropertyItemViewModelFactory(
        PropertyEditWizardProviderService propertyEditWizardProviderService)
    {
        public SmoothSetValueDefinitionPropertyItemViewModel Create(SmoothSetValuePropertyViewItemTerm term, int index,
            EditorNode nodeData, LocalServiceParam localServiceParam)
        {
            return new SmoothSetValueDefinitionPropertyItemViewModel(term, index, nodeData, localServiceParam,
                propertyEditWizardProviderService);
        }
    }
}
