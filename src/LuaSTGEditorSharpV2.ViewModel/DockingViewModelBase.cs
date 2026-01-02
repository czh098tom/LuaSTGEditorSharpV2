using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.ViewModel
{
    /// <summary>
    /// Base viewmodel for any docking panels
    /// </summary>
    public abstract class DockingViewModelBase(IServiceProvider serviceProvider) : InjectableViewModel(serviceProvider), IDisposable
    {
        public class PublishCommandEventArgs : EventArgs
        {
            public CommandBase? Command { get; set; }
            public IDocument? DocumentModel { get; set; }
            public EditorNode[] EditorNodes { get; set; } = [];
            public bool ShouldRefreshView { get; set; } = true;
        }

        public class SelectedNodeChangedEventArgs : EventArgs
        {
            public IDocument? DocumentModel { get; set; }
            public EditorNode[] EditorNodes { get; set; } = [];
        }

        public EditorNode[] SourceNodes { get; private set; } = [];

        public IDocument? SourceDocument { get; private set; }

        public event EventHandler? OnClose;
        public event EventHandler? OnReopen;

        private ICommand? _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                _closeCommand ??= new RelayCommand(Close);
                return _closeCommand;
            }
        }

        private bool _canClose = true;
        public bool CanClose
        {
            get { return _canClose; }
            set
            {
                if (_canClose != value)
                {
                    _canClose = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_isActive)
                    {
                        HandleOnSelect();
                    }
                    else
                    {
                        HandleOnDeselect();
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _disposedValue;

        public event EventHandler<PublishCommandEventArgs>? OnCommandPublishing;

        public abstract string Title { get; }

        public void Close()
        {
            OnClose?.Invoke(this, EventArgs.Empty);
        }

        public void Reopen()
        {
            OnReopen?.Invoke(this, EventArgs.Empty);
        }

        protected void PublishCommand(CommandBase? command, IDocument documentModel, EditorNode[] nodeData, bool shouldRefreshView = true)
        {
            OnCommandPublishing?.Invoke(this, new()
            {
                Command = command,
                DocumentModel = documentModel,
                EditorNodes = nodeData,
                ShouldRefreshView = shouldRefreshView
            });
        }

        public void HandleSelectedNodeChanged(object o, SelectedNodeChangedEventArgs args)
        {
            if (!ShouldChangeSelectedNode(o, args)) return;
            var doc = args.DocumentModel;
            SourceDocument = doc;
            var node = args.EditorNodes;
            SourceNodes = node;
            HandleSelectedNodeChangedImpl(o, args);
        }

        public virtual bool ShouldChangeSelectedNode(object o, SelectedNodeChangedEventArgs args)
        {
            return true;
        }

        public virtual void HandleSelectedNodeChangedImpl(object o, SelectedNodeChangedEventArgs args)
        {
        }

        protected virtual void HandleOnSelect() { }
        protected virtual void HandleOnDeselect() { }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~WorkSpaceViewModel()
        // {
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
