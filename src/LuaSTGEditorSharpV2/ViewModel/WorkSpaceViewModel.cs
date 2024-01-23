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

        public WorkSpaceCollection<DocumentViewModel> Documents { get; private set; } = [];

        public int ActiveDocumentIndex { get; set; } = 0;

        public WorkSpaceViewModel()
        {
        }

        public void AddPage(AnchorableViewModelBase viewModel)
        {
            viewModel.OnClose += (o, e) => MakeInvisible(o as AnchorableViewModelBase);
            viewModel.OnCommandPublishing += (o, e) => AddCommandToDocument(e.Command, e.NodeData, e.ShouldRefreshView);
            viewModel.OnFetchActiveDocument += FetchActiveDocument;
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

        public void BroadcastSelectedNodeChanged(NodeData? nodeData)
        {
            var doc = GetActiveDocument();
            foreach (var p in Anchorables)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = doc, NodeData = nodeData });
            }
            foreach (var p in _invisibleAnchorables)
            {
                p?.HandleSelectedNodeChanged(this, new() { DocumentModel = doc, NodeData = nodeData });
            }
        }

        private void AddCommandToDocument(CommandBase? command, NodeData? nodeData, bool shouldRefresh)
        {
            if (command == null) return;
            var docService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var doc = docService.ActiveDocuments[ActiveDocumentIndex];
            var param = new LocalServiceParam(doc);
            doc.CommandBuffer.Execute(command, param);
            if (shouldRefresh && nodeData != null)
            {
                BroadcastSelectedNodeChanged(nodeData);
            }
        }

        public void FetchActiveDocument(object? o, AnchorableViewModelBase.FetchActiveDocumentEventArgs args)
        {
            args.DocumentModel = GetActiveDocument();
        }

        private EditingDocumentModel GetActiveDocument()
        {
            var docService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var doc = docService.ActiveDocuments[ActiveDocumentIndex];
            return doc;
        }

        public void InsertNodeOfCustomType(NodeData nodeData, string typeUID)
        {
            var activeDocService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            EditingDocumentModel doc = activeDocService.ActiveDocuments[ActiveDocumentIndex];
            var param = new LocalServiceParam(doc);
            doc.CommandBuffer.Execute(new AddChildCommand(nodeData, nodeData.PhysicalChildren.Count, new NodeData(typeUID)), param);
        }
    }
}
