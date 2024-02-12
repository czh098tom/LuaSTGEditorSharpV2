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

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class WorkSpaceViewModel : ViewModelBase
    {
        private readonly WorkSpaceCollection<AnchorableViewModelBase> _invisibleAnchorables = [];
        public WorkSpaceCollection<AnchorableViewModelBase> Anchorables { get; private set; } = [];

        private readonly ObservableCollection<DocumentViewModel> _documents = [];
        public ObservableCollection<DocumentViewModel> Documents => _documents;

        private DocumentViewModel? _activeDocument;

        public NodeData[] SelectedNodes { get; private set; } = [];

        [MemberNotNullWhen(true, nameof(_activeDocument))]
        public bool HaveActiveDocument => _activeDocument != null;
        [MemberNotNullWhen(true, nameof(_activeDocument))]
        public bool HaveSelected => SelectedNodes.Length > 0 && _activeDocument != null;
        [MemberNotNullWhen(true, nameof(_activeDocument))]
        public bool HaveSelectedSingle => SelectedNodes.Length == 1 && _activeDocument != null;

        private readonly Dictionary<IDocument, DocumentViewModel> _documentMappting = [];


        public void AddPage(AnchorableViewModelBase viewModel)
        {
            viewModel.OnClose += (o, e) => MakeInvisible(o as AnchorableViewModelBase);
            viewModel.OnReopen += (o, e) => MakeVisible(o as AnchorableViewModelBase);
            viewModel.OnCommandPublishing += HandleAddCommandEvent;
            Anchorables.Add(viewModel);
        }

        private void MakeVisible(AnchorableViewModelBase? viewModel)
        {
            var index = _invisibleAnchorables.FindIndex(viewModel);
            if (index < 0) return;
            var page = _invisibleAnchorables[index];
            _invisibleAnchorables.RemoveAt(index);
            Anchorables.Add(page);
        }

        private void MakeInvisible(AnchorableViewModelBase? viewModel)
        {
            var index = Anchorables.FindIndex(viewModel);
            if (index < 0) return;
            var page = Anchorables[index];
            Anchorables.RemoveAt(index);
            _invisibleAnchorables.Add(page);
        }

        public void BroadcastSelectedNodeChanged(DocumentViewModel? dvm, NodeData[] nodeData)
        {
            BroadcastSelectedNodeChanged(dvm?.DocumentModel, nodeData);
        }

        public void BroadcastSelectedNodeChanged(IDocument? documentModel, NodeData[] nodeData)
        {
            if(documentModel == null)
            {
                _activeDocument = null;
            }
            else
            {
                _activeDocument = _documentMappting.GetValueOrDefault(documentModel);
            }
            SelectedNodes = nodeData;
            foreach (var p in Anchorables)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = documentModel, NodeData = nodeData });
            }
            foreach (var p in _invisibleAnchorables)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = documentModel, NodeData = nodeData });
            }
            foreach (var p in _documents)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = documentModel, NodeData = nodeData });
            }
        }

        private void AddCommandToDocument(CommandBase? command, IDocument? document, NodeData[] nodeData, bool shouldRefresh)
        {
            if (command == null || document == null) return;
            var dvm = _documentMappting!.GetValueOrDefault(document, null);
            if (dvm == null) return;
            dvm.ExecuteCommand(command);
            if (shouldRefresh)
            {
                BroadcastSelectedNodeChanged(document, nodeData);
            }
        }

        public void AddDocument(EditingDocumentModel editingDocumentModel)
        {
            var doc = editingDocumentModel;
            if (doc == null) return;
            var dvm = new DocumentViewModel(doc);
            _documents.Add(dvm);
            _documentMappting.Add(doc, dvm);
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
            _activeDocument.SaveOrAskingToSaveAs();
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
            _documentMappting.Remove(dvm.DocumentModel);
            dvm.CloseActiveDocument();

            DisposeOpenedDocument(dvm);
        }

        public void UndoActiveDocument()
        {
            if (!HaveActiveDocument) throw new InvalidOperationException();
            _activeDocument.Undo();
        }

        public void RedoActiveDocument()
        {
            if (!HaveActiveDocument) throw new InvalidOperationException();
            _activeDocument.Redo();
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
            AddCommandToDocument(SelectedNodes.SelectCommand(n =>
            {
                if (n.PhysicalParent == null) return null;
                return new RemoveChildCommand(n.PhysicalParent, n.PhysicalParent.PhysicalChildren.FindIndex(n));
            }), _activeDocument.DocumentModel, [], true);
        }

        public void CopySelectedNode()
        {
            if (!HaveSelected) throw new InvalidOperationException();
            var nodes = _activeDocument.DocumentModel.Root.FindPhysicalMinForestContaining(SelectedNodes);
            HostedApplicationHelper.GetService<ClipboardService>().CopyNode(nodes);
        }

        public void CutSelectedNode()
        {
            CopySelectedNode();
            DeleteSelectedNode();
        }

        public void PasteToSelectedNode()
        {
            var clipBoard = HostedApplicationHelper.GetService<ClipboardService>();
            if (!clipBoard.CheckHaveNodes()) throw new InvalidOperationException();
            if (!HaveSelected) throw new InvalidOperationException();
            var insCommandHost = HostedApplicationHelper.GetService<InsertCommandHostingService>();

            var clipBoardContent = clipBoard.GetNodes();

            AddCommandToDocument(SelectedNodes.SelectCommand(n =>
                clipBoardContent.SelectCommand(c => insCommandHost.InsertCommandFactory.CreateInsertCommand(n, c)))
                , _activeDocument.DocumentModel, SelectedNodes, true);
        }

        public async void ViewCode()
        {
            var str = await Task.Run(GenerateCodeForFirstSelectedNode);
            var dialog = new ViewCodeDialog(str).ShowDialog();
        }

        public async void ExportCode()
        {
            if(_activeDocument?.SourceDocument == null) throw new InvalidOperationException();
            var dialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                FileName = _activeDocument.DocumentModel.FileName,
                Filter = "*.*|*.*",
                InitialDirectory = _activeDocument.DocumentModel.FilePath ?? string.Empty,
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
            foreach (CodeData codeData in HostedApplicationHelper.GetService<CodeGeneratorServiceProvider>()
                .GenerateCode(root, new LocalServiceParam(_activeDocument.SourceDocument)))
            {
                yield return codeData;
            }
        }

        private void DisposeOpenedDocument(DocumentViewModel dvm)
        {
            foreach (var p in Anchorables)
            {
                if (p.SourceDocument == dvm.DocumentModel)
                {
                    p?.HandleSelectedNodeChanged(this, new() { DocumentModel = null, NodeData = [] });
                }
            }
            foreach (var p in _invisibleAnchorables)
            {
                if (p.SourceDocument == dvm.DocumentModel)
                {
                    p?.HandleSelectedNodeChanged(this, new() { DocumentModel = null, NodeData = [] });
                }
            }
            if (_activeDocument == dvm)
            {
                _activeDocument = null;
            }
        }

        private void HandleAddCommandEvent(object? o, DockingViewModelBase.PublishCommandEventArgs e)
        {
            AddCommandToDocument(e.Command, e.DocumentModel, e.NodeData, e.ShouldRefreshView);
        }
    }
}
