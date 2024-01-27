using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class DocumentViewModel : DockingViewModelBase
    {
        public ObservableCollection<NodeViewModel> Tree { get; private set; } = [];

        private readonly EditingDocumentModel _editingDocumentModel;
        public IDocument DocumentModel => _editingDocumentModel;

        private string _rawTitle = string.Empty;
        public string RawTitle => _rawTitle;
        public override string Title =>
            string.Format("{0}{1}", _rawTitle, _editingDocumentModel.IsModified ? " *" : string.Empty);

        public bool IsModified => _editingDocumentModel.IsModified;

        public DocumentViewModel(EditingDocumentModel documentModel)
        {
            _editingDocumentModel = documentModel;
            _rawTitle = documentModel.FileName;
            Tree.Add(HostedApplicationHelper.GetService<ViewModelProviderServiceProvider>()
                .CreateViewModelRecursive(documentModel.Root, new LocalServiceParam(documentModel)));
        }

        public void ExecuteCommand(CommandBase command)
        {
            _editingDocumentModel.CommandBuffer.Execute(command, new LocalServiceParam(DocumentModel));
            RaisePropertyChanged(nameof(Title));
        }

        public void Undo()
        {
            _editingDocumentModel.CommandBuffer.Undo(new LocalServiceParam(DocumentModel));
            RaisePropertyChanged(nameof(Title));
        }

        public void Redo()
        {
            _editingDocumentModel.CommandBuffer.Redo(new LocalServiceParam(DocumentModel));
            RaisePropertyChanged(nameof(Title));
        }
    }
}
