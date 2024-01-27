using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class WorkSpaceViewModel : ViewModelBase
    {
        private readonly WorkSpaceCollection<AnchorableViewModelBase> _invisibleAnchorables = [];
        public WorkSpaceCollection<AnchorableViewModelBase> Anchorables { get; private set; } = [];

        private readonly ObservableCollection<DocumentViewModel> _documents = [];
        public ObservableCollection<DocumentViewModel> Documents => _documents;

        private readonly Dictionary<DocumentViewModel, EditingDocumentModel> _documentMappting = [];

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

        public void BroadcastSelectedNodeChanged(DocumentViewModel dvm, NodeData nodeData)
        {
            var doc = _documentMappting.GetValueOrDefault(dvm);
            if (doc == null) return;
            BroadcastSelectedNodeChanged(doc, nodeData);
        }

        public void BroadcastSelectedNodeChanged(EditingDocumentModel? documentModel, NodeData? nodeData)
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

        private void AddCommandToDocument(CommandBase? command, DocumentModel? document, NodeData? nodeData, bool shouldRefresh)
        {
            if (command == null) return;
            if (document is not EditingDocumentModel doc) return;
            var param = new LocalServiceParam(doc);
            doc.CommandBuffer.Execute(command, param);
            if (shouldRefresh && nodeData != null)
            {
                BroadcastSelectedNodeChanged(doc, nodeData);
            }
        }

        public void AddDocument(EditingDocumentModel editingDocumentModel)
        {
            var doc = editingDocumentModel;
            if (doc == null) return;
            var dvm = new DocumentViewModel(Path.GetFileName(doc.FilePath) ?? string.Empty);
            _documents.Add(dvm);
            _documentMappting.Add(dvm, doc);
            dvm.Tree.Add(HostedApplicationHelper.GetService<ViewModelProviderServiceProvider>().CreateViewModelRecursive(doc.Root, new LocalServiceParam(doc)));
        }
    }
}
