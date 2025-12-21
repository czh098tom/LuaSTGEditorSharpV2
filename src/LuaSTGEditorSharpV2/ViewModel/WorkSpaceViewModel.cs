using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

using System.Reactive.Disposables;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.WPF;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Command.Service;
using LuaSTGEditorSharpV2.Dialog;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Building.BuildTasks;
using LuaSTGEditorSharpV2.Core.Building;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class WorkSpaceViewModel : InjectableViewModel, IDisposable
    {
        public WorkSpaceCollection<AnchorableViewModelBase> Anchorables { get; private set; } = [];

        private readonly ObservableCollection<DocumentViewModel> _documents = [];
        public ObservableCollection<DocumentViewModel> Documents => _documents;

        private DocumentViewModel? _activeDocument;
        private readonly Dictionary<IDocument, DocumentViewModel> _documentMapping = [];

        public QueuedBoolHandle IsEnabledHandle { get; private set; }
        public bool IsEnabled
        {
            get => IsEnabledHandle.Value;
        }

        public EditorNode[] SelectedNodes { get; private set; } = [];

        [MemberNotNullWhen(true, nameof(_activeDocument))]
        public bool HaveActiveDocument => _activeDocument != null;
        [MemberNotNullWhen(true, nameof(_activeDocument))]
        public bool HaveSelected => SelectedNodes.Length > 0 && _activeDocument != null;
        [MemberNotNullWhen(true, nameof(_activeDocument))]
        public bool HaveSelectedSingle => SelectedNodes.Length == 1 && _activeDocument != null;

        public event EventHandler<OnEnableHandleRequestedEventArgs>? EnableRequesting;

        private bool _disposedValue;

        public WorkSpaceViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            IsEnabledHandle = new(this, nameof(IsEnabled));
        }

        public T AddOrActivatePage<T>() where T : AnchorableViewModelBase
        {
            return (T)AddOrActivatePage(typeof(T));
        }

        public AnchorableViewModelBase AddOrActivatePage(Type type)
        {
            if (Anchorables.FirstOrDefault(anc => anc.GetType() == type) is AnchorableViewModelBase visible)
            {
                return visible;
            }
            var result = (AnchorableViewModelBase)ServiceProvider.GetRequiredService(type);
            AddPage(result);
            return result;
        }

        public void AddHidedIfNotPresented(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                if (Anchorables.FirstOrDefault(anc => anc.GetType() == type) == null)
                {
                    var vm = (AnchorableViewModelBase)ServiceProvider.GetRequiredService(type);
                    AddPage(vm);
                    vm.IsVisible = false;
                }
            }
        }

        public void AddPage(AnchorableViewModelBase viewModel)
        {
            viewModel.IsActive = true;
            //viewModel.OnClose += (o, e) => MakeInvisible(o as AnchorableViewModelBase);
            //viewModel.OnReopen += (o, e) => MakeVisible(o as AnchorableViewModelBase);
            viewModel.OnCommandPublishing += HandleAddCommandEvent;
            Anchorables.Add(viewModel);
        }

        public AnchorableViewModelBase ChangeActiveState(Type type)
        {
            if (Anchorables.FirstOrDefault(anc => anc.GetType() == type) is AnchorableViewModelBase visible)
            {
                visible.IsVisible = !visible.IsVisible;
                return visible;
            }
            var result = (AnchorableViewModelBase)ServiceProvider.GetRequiredService(type);
            AddPage(result);
            return result;
        }

        public void BroadcastSelectedNodeChanged(DocumentViewModel? dvm, EditorNode[] editorNode)
        {
            BroadcastSelectedNodeChanged(dvm?.Document, editorNode);
        }

        public void BroadcastSelectedNodeChanged(IDocument? documentModel, EditorNode[] editorNode)
        {
            if (documentModel == null)
            {
                _activeDocument = null;
            }
            else
            {
                _activeDocument = _documentMapping.GetValueOrDefault(documentModel);
            }
            SelectedNodes = editorNode;
            foreach (var p in Anchorables)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = documentModel, EditorNodes = editorNode });
            }
            foreach (var p in _documents)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = documentModel, EditorNodes = editorNode });
            }
        }

        private void AddCommandToDocument(CommandBase? command, IDocument? document, EditorNode[] editorNode, bool shouldRefresh)
        {
            if (command == null || document == null) return;
            var dvm = _documentMapping!.GetValueOrDefault(document, null);
            if (dvm == null) return;
            dvm.ExecuteCommand(command);
            if (shouldRefresh)
            {
                BroadcastSelectedNodeChanged(document, editorNode);
            }
        }

        public void AddDocument(EditorDocument editingDocumentModel)
        {
            var doc = editingDocumentModel;
            if (doc == null) return;
            var dvm = ServiceProvider.GetRequiredService<DocumentViewModelFactory>().Create(doc);
            _documents.Add(dvm);
            _documentMapping.Add(doc, dvm);
            dvm.SelectedNodeChanged += BroadcastSelectedNodeChanged;
            dvm.OnClose += (o, e) => CloseDocument(dvm);
            dvm.OnCommandPublishing += HandleAddCommandEvent;
        }

        public void SetActiveDocument(DocumentViewModel dvm)
        {
            _activeDocument = dvm;
        }

        public void SaveActiveDocument()
        {
            if (!HaveActiveDocument) throw new InvalidOperationException();
            _activeDocument.SaveOrAskToSaveAs();
        }

        public void SaveActiveDocumentAs()
        {
            if (!HaveActiveDocument) throw new InvalidOperationException();
            _activeDocument.SaveAs();
        }

        public void CloseDocument(DocumentViewModel dvm)
        {
            if (!dvm.CanClose) return;
            if (dvm.IsModified)
            {
                dvm.AskSaveBeforeClose();
            }
            _documents.Remove(dvm);
            _documentMapping.Remove(dvm.Document);
            dvm.CloseActiveDocument();

            DisposeOpenedDocument(dvm);
        }

        public void UndoActiveDocument()
        {
            if (!HaveActiveDocument) throw new InvalidOperationException();
            _activeDocument.Undo();
            BroadcastSelectedNodeChanged(_activeDocument, SelectedNodes);
        }

        public void RedoActiveDocument()
        {
            if (!HaveActiveDocument) throw new InvalidOperationException();
            _activeDocument.Redo();
            BroadcastSelectedNodeChanged(_activeDocument, SelectedNodes);
        }

        public bool CanPerformUndoActivateDocument()
        {
            return _activeDocument?.CanUndo ?? false;
        }

        public bool CanPerformRedoActivateDocument()
        {
            return _activeDocument?.CanRedo ?? false;
        }

        public void DeleteSelectedNode()
        {
            if (!HaveSelected) throw new InvalidOperationException();
            AddCommandToDocument(SelectedNodes.SelectFilter(CheckedCommand.RemoveNode), 
                _activeDocument.Document, [], true);
        }

        public void CopySelectedNode()
        {
            if (!HaveSelected) throw new InvalidOperationException();
            var nodes = _activeDocument.Document.Root.FindPhysicalMinForestContaining([.. SelectedNodes.Select(en => en.Source)]);
            ServiceProvider.GetRequiredService<ClipboardService>().CopyNode(nodes);
        }

        public void CutSelectedNode()
        {
            CopySelectedNode();
            DeleteSelectedNode();
        }

        public void PasteToSelectedNode()
        {
            var clipBoard = ServiceProvider.GetRequiredService<ClipboardService>();
            if (!clipBoard.CheckHaveNodes()) throw new InvalidOperationException();
            if (!HaveSelected) throw new InvalidOperationException();
            var insCommandHost = ServiceProvider.GetRequiredService<InsertCommandHostingService>();

            var clipBoardContent = clipBoard.GetNodes();

            AddCommandToDocument(SelectedNodes.SelectFilter(n =>
                insCommandHost.InsertCommandFactory.CreateInsertCommand(n, clipBoardContent))
                , _activeDocument.Document, SelectedNodes, true);
        }

        public async void ViewCode()
        {
            var str = await Task.Run(GenerateCodeForFirstSelectedNode);
            var dialog = new ViewCodeDialog(str).ShowDialog();
        }

        public async void ExportCode()
        {
            if (_activeDocument?.SourceDocument == null) throw new InvalidOperationException();
            var dialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                FileName = _activeDocument.Document.FileName,
                Filter = "*.*|*.*",
                InitialDirectory = _activeDocument.Document.FilePath ?? string.Empty,
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            var fileName = dialog.FileName;
            await Task.Run(() =>
            {
                using FileStream fs = new(fileName, FileMode.Create, FileAccess.Write);
                using StreamWriter sw = new(fs);
                foreach (var codedata in EnumerateCodeForFirstSelectedNode())
                {
                    sw.Write(codedata.Content);
                }
            });
        }

        public async void ExecuteBuildForSelected()
        {
            if (_activeDocument?.SourceDocument == null) throw new InvalidOperationException();
            if (!_activeDocument.SaveOrAskToSaveAs()) return;

            var selectedDoc = _activeDocument.SourceDocument;

            var taskFactoryService = ServiceProvider.GetRequiredService<BuildTaskFactoryServiceProvider>();
            var param = new LocalServiceParam(selectedDoc);
            var buildingContext = ServiceProvider.GetRequiredService<BuildingContextFactory>().Create(param);

            using var _ = new CompositeDisposable(RaiseEnableRequestingEvent());
            await Task.WhenAll(SelectedNodes
                .Select(n => taskFactoryService.GetWeightedBuildingTaskForNode(n.Source, param)?.BuildingTask)
                .OfType<NamedBuildingTask>()
                .Select(t => t.Execute(buildingContext)));
        }

        private string GenerateCodeForFirstSelectedNode()
        {
            var sb = new StringBuilder();
            foreach (CodeData codeData in EnumerateCodeForFirstSelectedNode())
            {
                sb.Append(codeData.Content);
            }
            return sb.ToString();
        }

        private IEnumerable<CodeData> EnumerateCodeForFirstSelectedNode()
        {
            if (!HaveSelected || SelectedNodes.Length != 1 || _activeDocument.SourceDocument == null)
            {
                throw new InvalidOperationException();
            }
            var root = SelectedNodes[0];
            foreach (CodeData codeData in ServiceProvider.GetRequiredService<CodeGeneratorServiceProvider>()
                .GenerateCode(root.Source, new LocalServiceParam(_activeDocument.SourceDocument)))
            {
                yield return codeData;
            }
        }

        public bool CanPerformBuild()
        {
            if (_activeDocument == null) return false;
            if (_activeDocument.SourceDocument == null) return false;
            var taskFactoryService = ServiceProvider.GetRequiredService<BuildTaskFactoryServiceProvider>();
            var selectedDoc = _activeDocument.SourceDocument;
            var param = new LocalServiceParam(selectedDoc);
            return SelectedNodes.Any(n => taskFactoryService.GetWeightedBuildingTaskForNode(n.Source, param)
                ?.BuildingTask is NamedBuildingTask);
        }

        private void DisposeOpenedDocument(DocumentViewModel dvm)
        {
            foreach (var p in Anchorables)
            {
                if (p.SourceDocument == dvm.Document)
                {
                    p?.HandleSelectedNodeChanged(this, new() { DocumentModel = null, EditorNodes = [] });
                }
            }
            if (_activeDocument == dvm)
            {
                _activeDocument = null;
            }
        }

        private void HandleAddCommandEvent(object? o, DockingViewModelBase.PublishCommandEventArgs e)
        {
            AddCommandToDocument(e.Command, e.DocumentModel, e.EditorNodes, e.ShouldRefreshView);
        }

        private IEnumerable<IDisposable> RaiseEnableRequestingEvent()
        {
            var args = new OnEnableHandleRequestedEventArgs();
            EnableRequesting?.Invoke(this, args);
            return args.Disposables;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var anc in Anchorables)
                    {
                        anc.Dispose();
                    }
                    Anchorables.Clear();
                }

                _disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~WorkSpaceViewModel()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
