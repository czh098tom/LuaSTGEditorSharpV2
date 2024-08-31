using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ViewModel
{
    /// <summary>
    /// Base viewmodel for any docking panels
    /// </summary>
    public abstract class DockingViewModelBase(IServiceProvider serviceProvider) : InjectableViewModel(serviceProvider)
    {
        public class PublishCommandEventArgs : EventArgs
        {
            public CommandBase? Command { get; set; }
            public IDocument? DocumentModel { get; set; }
            public NodeData[] NodeData { get; set; } = [];
            public bool ShouldRefreshView { get; set; } = true;
        }

        public class SelectedNodeChangedEventArgs : EventArgs
        {
            public IDocument? DocumentModel { get; set; }
            public NodeData[] NodeData { get; set; } = [];
        }

        public NodeData[] SourceNodes { get; private set; } = [];

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
                        Reopen();
                    }
                    else
                    {
                        Close();
                    }
                    RaisePropertyChanged();
                }
            }
        }

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

        protected void PublishCommand(CommandBase? command, IDocument documentModel, NodeData[] nodeData, bool shouldRefreshView = true)
        {
            OnCommandPublishing?.Invoke(this, new()
            {
                Command = command,
                DocumentModel = documentModel,
                NodeData = nodeData,
                ShouldRefreshView = shouldRefreshView
            });
        }

        public void HandleSelectedNodeChanged(object o, SelectedNodeChangedEventArgs args)
        {
            if (!ShouldChangeSelectedNode(o, args)) return;
            var doc = args.DocumentModel;
            SourceDocument = doc;
            var node = args.NodeData;
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
    }
}
