using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.WPF;
using System.Windows;

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

        public void SaveOrAskingToSaveAs()
        {
            if (DocumentModel.IsOnDisk())
            {
                Save();
            }
            else
            {
                SaveAs();
            }
        }

        private void Save()
        {
            DocumentModel.Save();
            RaisePropertyChanged(nameof(Title));
        }

        public void SaveAs()
        {
            var fileDialog = HostedApplicationHelper.GetService<FileDialogService>();
            string? path = fileDialog.ShowSaveAsFileCommandDialog();
            if (path == null) return;
            DocumentModel.SaveAs(path);
            RaisePropertyChanged(nameof(Title));
        }

        public void AskSaveBeforeClose()
        {
            var localization = HostedApplicationHelper.GetService<LocalizationService>();
            var messageBoxResult =
                MessageBox.Show(
                    string.Format(localization.GetString("messageBox_saveBeforClose_message",
                        typeof(WorkSpaceViewModel).Assembly), RawTitle),
                    localization.GetString("messageBox_title_app", typeof(WindowHelper).Assembly),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Information
                    );
            if (messageBoxResult == MessageBoxResult.Cancel) return;
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                SaveOrAskingToSaveAs();
            }
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
    }
}
