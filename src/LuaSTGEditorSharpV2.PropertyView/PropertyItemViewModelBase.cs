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

        public IReadOnlyList<EditorNode> SourceNodes { get; private set; } = [];
        public LocalServiceParam LocalServiceParam { get; private set; } = null!;
        public PropertyEditWizardProviderService WizardProviderService { get; private set; } = null!;

        public event EventHandler<EditResult>? OnEdit;

        public ICommand? ShowEditWindow { get; protected set; }

        private bool initializedValue;
        private bool disposedValue;

        public void Initialize(IReadOnlyList<EditorNode> editorNode,
            LocalServiceParam localServiceParam, PropertyEditWizardProviderService wizardProviderService)
        {
            if (initializedValue)
            {
                throw new InvalidOperationException($"{GetType().Name} has already been initialized.");
            }
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            SourceNodes = editorNode;
            LocalServiceParam = localServiceParam;
            WizardProviderService = wizardProviderService;
            foreach (var sourceNode in SourceNodes)
            {
                sourceNode.OnPropertyChanged += HandleEditorNodeOnPropertyChanged;
            }
            initializedValue = true;
        }

        protected abstract void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e);

        public void RaiseOnEdit(EditResult editResult)
        {
            OnEdit?.Invoke(this, editResult);
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
