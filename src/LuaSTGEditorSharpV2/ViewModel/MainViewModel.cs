using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Services;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private PropertyPageViewModel _propertyPage = new();

        public PropertyPageViewModel PropertyPage
        {
            get => _propertyPage;
        }

        public ObservableCollection<DocumentViewModel> Documents { get; private set; } = [];

        public int ActiveDocumentIndex { get; set; } = 0;

        public MainViewModel()
        {
            PropertyPage.OnTabItemValueUpdated += PropertyPageTabItemValueUpdatedHandler;
        }

        private void PropertyPageTabItemValueUpdatedHandler(object? sender, PropertyPageViewModel.TabItemValueUpdatedEventArgs e)
        {
            var docService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var propertyViewService = HostedApplicationHelper.GetService<PropertyViewServiceProvider>();
            var doc = docService.ActiveDocuments[ActiveDocumentIndex];
            var param = new LocalServiceParam(doc);
            var command = propertyViewService.GetCommandOfEditingNode(
                PropertyPage.Source ?? NodeData.Empty,
                param, PropertyPage.Tabs, PropertyPage.Tabs.IndexOf(e.Tab),
                e.Tab.Properties.IndexOf(e.Args.Item),
                e.Args.Args.NewValue);
            if (command != null)
            {
                doc.CommandBuffer.Execute(command, param);
            }
            var list = propertyViewService.GetPropertyViewModelOfNode(PropertyPage.Source ?? NodeData.Empty, param);
            PropertyPage.LoadProperties(list, PropertyPage.Source ?? NodeData.Empty);
        }

        public void OpenFile(string filePath)
        {
            var activeDocService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var doc = activeDocService.Open(filePath);
            if (doc == null) return;
            var dvm = new DocumentViewModel(Path.GetFileName(filePath));
            Documents.Add(dvm);
            dvm.Tree.Add(HostedApplicationHelper.GetService<ViewModelProviderServiceProvider>().CreateViewModelRecursive(doc.Root, new LocalServiceParam(doc)));
        }

        public void LoadProperties(NodeData nodeData)
        {
            var activeDocService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var param = new LocalServiceParam(activeDocService.ActiveDocuments[ActiveDocumentIndex]);
            var list = HostedApplicationHelper.GetService<PropertyViewServiceProvider>().GetPropertyViewModelOfNode(nodeData, param);
            PropertyPage.LoadProperties(list, nodeData);
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
