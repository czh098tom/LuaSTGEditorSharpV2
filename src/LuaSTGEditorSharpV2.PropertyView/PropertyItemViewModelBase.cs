using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public abstract class PropertyItemViewModelBase(EditorNode nodeData,
        LocalServiceParam localServiceParam, 
        PropertyEditWizardProviderService wizardProviderService) : ViewModelBase
    {
        private string _value = string.Empty;
        private PropertyViewEditorType? _type;

        public string Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                _value = value;
                RaisePropertyChanged();
                OnEdit?.Invoke(this, ResolveEditingNodeCommand(SourceNode.Source, LocalServiceParam, value));
            }
        }

        private bool _enabled = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                RaisePropertyChanged();
            }
        }

        public PropertyViewEditorType? Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }

        public EditorNode SourceNode { get; private init; } = nodeData;
        public LocalServiceParam LocalServiceParam { get; private init; } = localServiceParam;
        public PropertyEditWizardProviderService WizardProviderService { get; } = wizardProviderService;

        public event EventHandler<EditResult>? OnEdit;

        public ICommand? ShowEditWindow { get; protected set; }

        protected void RaiseOnEdit(EditResult editResult)
        {
            OnEdit?.Invoke(this, editResult);
        }

        public abstract EditResult ResolveEditingNodeCommand(NodeData nodeData,
            LocalServiceParam context, string edited);
    }
}
