using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Command.Service;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Editor.Extension;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.WPF;
using LuaSTGEditorSharpV2.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DocumentViewModel : DockingViewModelBase
    {
        public delegate void SelectedNodeChangedHandler(DocumentViewModel? dvm, EditorNode[] editorNode);

        private readonly EditorDocument _editingDocumentModel;

        public ObservableCollection<DocumentTabViewModel> Tabs { get; private set; } = [];
        public ObservableCollection<NodeViewModel> Tree { get; private set; } = [];

        private object? _selectedNode;
        public object? SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                if (_selectedNode is not IEnumerable nodes)
                {
                    _selectedNodeStrongTyped = [];
                    SelectedNodeChanged?.Invoke(this, []);
                }
                else
                {
                    var nodeList = ProcessSelectedNodes(nodes).ToArray();
                    _selectedNodeStrongTyped = nodeList;
                    SelectedNodeChanged?.Invoke(this, nodeList);
                }
                RaisePropertyChanged();
            }
        }

        private EditorNode[] _selectedNodeStrongTyped = [];

        public event SelectedNodeChangedHandler? SelectedNodeChanged;

        public bool HasTabs => Tabs.Count > 0;

        public IDocument Document => _editingDocumentModel;

        private string _rawTitle = string.Empty;
        public string RawTitle => _rawTitle;
        public override string Title =>
            string.Format("{0}{1}", _rawTitle, _editingDocumentModel.IsModified ? " *" : string.Empty);

        public bool IsModified => _editingDocumentModel.IsModified;

        public bool CanUndo => _editingDocumentModel.CanUndo;
        public bool CanRedo => _editingDocumentModel.CanRedo;

        public DocumentViewModel(IServiceProvider serviceProvider, EditorDocument documentModel) : base(serviceProvider)
        {
            _editingDocumentModel = documentModel;
            _rawTitle = documentModel.FileName;
            var vm = documentModel.RootEditorNode.GetRequiredNodeService<NodeViewModel>();
            Tree.Add(vm);
            foreach (var child in vm.Children)
            {
                Tabs.Add(new DocumentTabViewModel(child));
            }
            documentModel.RootEditorNode.OnChildrenChanged += RootEditorNode_OnChildrenChanged;
            Tabs.CollectionChanged += Tabs_CollectionChanged;
        }

        /// <summary>
        /// If the document is already on disk, save the file.
        /// Otherwise ask user the path where the document should be saved.
        /// </summary>
        /// <returns> 
        /// <see cref="true"/> if Document has saved, otherwise <see cref="false"/>. 
        /// </returns>
        public bool SaveOrAskToSaveAs()
        {
            if (Document.IsOnDisk())
            {
                Save();
                return true;
            }
            else
            {
                return SaveAs();
            }
        }

        private void Save()
        {
            Document.Save();
            RaisePropertyChanged(nameof(Title));
        }

        /// <summary>
        /// Ask user the path where the document should be saved, then save the document to that directory.
        /// </summary>
        /// <returns> 
        /// <see cref="true"/> if Document has saved, otherwise <see cref="false"/>. 
        /// </returns>
        public bool SaveAs()
        {
            var fileDialog = ServiceProvider.GetRequiredService<FileDialogService>();
            string? path = fileDialog.ShowSaveAsFileCommandDialog(Document.FileName);
            if (path == null) return false;
            Document.SaveAs(path);
            RaisePropertyChanged(nameof(Title));
            return true;
        }

        public bool AskSaveBeforeClose()
        {
            var localization = ServiceProvider.GetRequiredService<LocalizationService>();
            var messageBoxResult =
                MessageBox.Show(
                    string.Format(localization.GetString("messageBox_saveBeforClose_message",
                        typeof(WorkSpaceViewModel).Assembly), RawTitle),
                    localization.GetString("messageBox_title_app", typeof(WindowHelper).Assembly),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Information
                    );
            if (messageBoxResult == MessageBoxResult.Cancel) return false;
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                return SaveOrAskToSaveAs();
            }
            return true;
        }

        public void CloseActiveDocument()
        {
            var activeDocService = ServiceProvider.GetRequiredService<ActiveDocumentService>();
            activeDocService.Close(_editingDocumentModel);
            activeDocService.MarkAsSaved(_editingDocumentModel);
        }

        public void ExecuteCommand(CommandBase command)
        {
            _editingDocumentModel.ExecuteCommand(command);
            RaisePropertyChanged(nameof(Title));
        }

        public void Undo()
        {
            _editingDocumentModel.Undo();
            RaisePropertyChanged(nameof(Title));
        }

        public void Redo()
        {
            _editingDocumentModel.Redo();
            RaisePropertyChanged(nameof(Title));
        }

        private IEnumerable<EditorNode> ProcessSelectedNodes(IEnumerable nodes)
        {
            IEnumerable<EditorNode> Get()
            {
                foreach (var item in nodes)
                {
                    if (item is NodeViewModel nvm)
                    {
                        yield return nvm.EditorNode;
                    }
                }
            }

            return _editingDocumentModel.OrderByViewOrder(Get());
        }

        protected override void HandleOnSelect()
        {
            base.HandleOnSelect();
            SelectedNodeChanged?.Invoke(this, _selectedNodeStrongTyped);
        }

        private void RootEditorNode_OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                int i = e.NewStartingIndex;
                foreach (EditorNode en in e.NewItems!)
                {
                    Tabs.Insert(i, new(en.ServiceProvider.GetRequiredKeyedService<NodeViewModel>(ScopeKey.EditorNode)));
                    i++;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (EditorNode en in e.OldItems!)
                {
                    Tabs.Remove(Tabs.First(t => t.Header.EditorNode == en));
                }
            }
        }

        private void Tabs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(HasTabs));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _editingDocumentModel.RootEditorNode.OnChildrenChanged -= RootEditorNode_OnChildrenChanged;
            Tabs.CollectionChanged -= Tabs_CollectionChanged;
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class DocumentViewModelFactory(IServiceProvider serviceProvider)
    {
        public DocumentViewModel Create(EditorDocument documentModel)
            => new(serviceProvider, documentModel);
    }
}
