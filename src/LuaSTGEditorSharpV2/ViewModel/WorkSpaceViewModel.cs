using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.WPF;
using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class WorkSpaceViewModel : ViewModelBase
    {
        private readonly WorkSpaceCollection<AnchorableViewModelBase> _invisibleAnchorables = [];
        public WorkSpaceCollection<AnchorableViewModelBase> Anchorables { get; private set; } = [];

        private readonly ObservableCollection<DocumentViewModel> _documents = [];
        public ObservableCollection<DocumentViewModel> Documents => _documents;

        private DocumentViewModel? _activeDocument;
        public bool HaveActiveDocument => _activeDocument != null;

        private readonly Dictionary<IDocument, DocumentViewModel> _documentMappting = [];

        public void AddPage(AnchorableViewModelBase viewModel)
        {
            viewModel.OnClose += (o, e) => MakeInvisible(o as AnchorableViewModelBase);
            viewModel.OnReopen += (o, e) => MakeVisible(o as AnchorableViewModelBase);
            viewModel.OnCommandPublishing += (o, e) => AddCommandToDocument(e.Command, e.DocumentModel, e.NodeData, e.ShouldRefreshView);
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

        public void BroadcastSelectedNodeChanged(DocumentViewModel dvm, NodeData[] nodeData)
        {
            BroadcastSelectedNodeChanged(dvm?.DocumentModel, nodeData);
        }

        public void BroadcastSelectedNodeChanged(IDocument? documentModel, NodeData[] nodeData)
        {
            foreach (var p in Anchorables)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = documentModel, NodeData = nodeData });
            }
            foreach (var p in _invisibleAnchorables)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = documentModel, NodeData = nodeData });
            }
        }

        private void AddCommandToDocument(CommandBase? command, IDocument? document, NodeData? nodeData, bool shouldRefresh)
        {
            if (command == null || document == null) return;
            var dvm = _documentMappting!.GetValueOrDefault(document, null);
            if (dvm == null) return;
            dvm.ExecuteCommand(command);
            if (shouldRefresh && nodeData != null)
            {
                BroadcastSelectedNodeChanged(document, [nodeData]);
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
        }

        public void SetActiveDocument(DocumentViewModel dvm)
        {
            _activeDocument = dvm;
        }

        public void SaveActiveDocument()
        {
            if (_activeDocument == null) throw new InvalidOperationException();
            _activeDocument?.SaveOrAskingToSaveAs();
        }

        public void SaveActiveDocumentAs()
        {
            if (_activeDocument == null) throw new InvalidOperationException();
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

        private void DisposeOpenedDocument(DocumentViewModel dvm)
        {
            foreach (var p in Anchorables)
            {
                if (p.SourceDocument == dvm.DocumentModel)
                {
                    p?.HandleSelectedNodeChanged(this, new() { DocumentModel = null, NodeData = null });
                }
            }
            foreach (var p in _invisibleAnchorables)
            {
                if (p.SourceDocument == dvm.DocumentModel)
                {
                    p?.HandleSelectedNodeChanged(this, new() { DocumentModel = null, NodeData = null });
                }
            }
            if (_activeDocument == dvm)
            {
                _activeDocument = null;
            }
        }

        public void UndoActiveDocument()
        {
            if (_activeDocument == null) throw new InvalidOperationException();
            _activeDocument.Undo();
        }

        public void RedoActiveDocument()
        {
            if (_activeDocument == null) throw new InvalidOperationException();
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
    }
}
