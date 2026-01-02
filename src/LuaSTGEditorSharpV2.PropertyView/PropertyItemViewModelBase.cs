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
        public string Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                _value = value;
                if (oldValue != value)
                {
                    RaisePropertyChanged();
                    OnEdit?.Invoke(this, ResolveBatchEditingNodeCommand(SourceNodes, LocalServiceParam, value));
                }
            }
        }
        private string _value = string.Empty;

        public BatchEditStatus BatchEditStatus
        {
            get => _batchEditStatus;
            set
            {
                _batchEditStatus = value;
                RaisePropertyChanged();
            }
        }
        private BatchEditStatus _batchEditStatus = BatchEditStatus.AllSame;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                RaisePropertyChanged();
            }
        }
        private bool _enabled = true;

        public PropertyViewEditorType? Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }
        private PropertyViewEditorType? _type;

        public IReadOnlyList<EditorNode> SourceNodes { get; private init; }
        public LocalServiceParam LocalServiceParam { get; private init; }
        public PropertyEditWizardProviderService WizardProviderService { get; }

        public event EventHandler<EditResult>? OnEdit;

        public ICommand? ShowEditWindow { get; protected set; }

        private bool disposedValue;

        public PropertyItemViewModelBase(IReadOnlyList<EditorNode> editorNode,
            BatchEditStatus isBatchSame,
            LocalServiceParam localServiceParam, PropertyEditWizardProviderService wizardProviderService)
        {
            SourceNodes = editorNode;
            LocalServiceParam = localServiceParam;
            WizardProviderService = wizardProviderService;
            BatchEditStatus = isBatchSame;
            foreach (var sourceNode in SourceNodes)
            {
                sourceNode.OnPropertyChanged += HandleEditorNodeOnPropertyChanged;
            }
        }

        protected abstract void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e);

        protected void RaiseOnEdit(EditResult editResult)
        {
            OnEdit?.Invoke(this, editResult);
        }

        public abstract EditResult ResolveBatchEditingNodeCommand(IReadOnlyList<EditorNode> nodeData,
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
                    foreach (var sourceNode in SourceNodes)
                    {
                        sourceNode.OnPropertyChanged -= HandleEditorNodeOnPropertyChanged;
                    }
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
