using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Xaml.Behaviors.Core;

using Newtonsoft.Json;

using Xceed.Wpf.AvalonDock.Controls;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel vm = new();
        private readonly EditingDocumentModel doc;

        public ICommand CommitEdit { get; private set; }
        public ICommand ShowEditWindow { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            InitializeCommand();

            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test");

            try
            {
                doc = new EditingDocumentModel(DocumentModel.CreateFromFile(Path.Combine(testPath, "test.lstgxml")));
                var dvm = new DocumentViewModel();
                vm.Documents.Add(dvm);
                DataContext = vm;

                dvm.Tree.Add(HostedApplicationHelper.GetService<ViewModelProviderServiceProvider>().CreateViewModelRecursive(doc.Root, new LocalServiceParam(doc)));

                vm.PropertyPage.OnTabItemValueUpdated += (o, e) =>
                {
                    var param = new LocalServiceParam(doc);
                    var command = HostedApplicationHelper.GetService<PropertyViewServiceProvider>().GetCommandOfEditingNode(
                        vm.PropertyPage.Source ?? NodeData.Empty,
                        param, vm.PropertyPage.Tabs, vm.PropertyPage.Tabs.IndexOf(e.Tab),
                        e.Tab.Properties.IndexOf(e.Args.Item),
                        e.Args.Args.NewValue);
                    doc.CommandBuffer.Execute(command, param);
                    var list = HostedApplicationHelper.GetService<PropertyViewServiceProvider>().GetPropertyViewModelOfNode(vm.PropertyPage.Source ?? NodeData.Empty, param);
                    vm.PropertyPage.LoadProperties(list, vm.PropertyPage.Source ?? NodeData.Empty);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (doc == null) throw new FileNotFoundException();
        }

        [MemberNotNull(nameof(CommitEdit))]
        [MemberNotNull(nameof(ShowEditWindow))]
        private void InitializeCommand()
        {
            // TODO[*] remove this
            CommitEdit = new ActionCommand(args =>
            {
            });
            ShowEditWindow = new ActionCommand(args => { });
        }

        private void PropertyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            var param = new LocalServiceParam(doc);
            if (tree?.SelectedItem is NodeViewModel selectedVM)
            {
                var list = HostedApplicationHelper.GetService<PropertyViewServiceProvider>().GetPropertyViewModelOfNode(selectedVM.Source, param);
                vm.PropertyPage.LoadProperties(list, selectedVM.Source);
                _propertyTab.SelectedIndex = 0;
            }
        }
    }
}
