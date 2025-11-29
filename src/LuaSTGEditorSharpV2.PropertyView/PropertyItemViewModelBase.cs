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
    public abstract class PropertyItemViewModelBase : ViewModelBase, IDisposable
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
                OnEdit?.Invoke(this, ResolveEditingNodeCommand(SourceNode, LocalServiceParam, value));
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

        public EditorNode SourceNode { get; private init; }
        public LocalServiceParam LocalServiceParam { get; private init; }
        public PropertyEditWizardProviderService WizardProviderService { get; }

        public event EventHandler<EditResult>? OnEdit;

        public ICommand? ShowEditWindow { get; protected set; }

        private bool disposedValue;

        public PropertyItemViewModelBase(EditorNode editorNode,
            LocalServiceParam localServiceParam,
            PropertyEditWizardProviderService wizardProviderService)
        {
            SourceNode = editorNode;
            LocalServiceParam = localServiceParam;
            WizardProviderService = wizardProviderService;
            editorNode.OnPropertyChanged += HandleEditorNodeOnPropertyChanged;
        }

        protected abstract void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e);

        protected void RaiseOnEdit(EditResult editResult)
        {
            OnEdit?.Invoke(this, editResult);
        }

        public abstract EditResult ResolveEditingNodeCommand(EditorNode nodeData,
            LocalServiceParam context, string edited);

        protected void SetValueWithoutPushingEditCommand(string value)
        {
            _value = value;
            RaisePropertyChanged(nameof(Value));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
