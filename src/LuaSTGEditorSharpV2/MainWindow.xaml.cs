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

using LuaSTGEditorSharpV2.Core.ViewModel;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;

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

                ViewModelProviderServiceBase.CreateRoot(dvm.Tree, doc.Root);
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
            CommitEdit = new ActionCommand(args =>
            {
                if (args is not RoutedEventArgs rea
                    || rea.Source is not TextBox textBox
                    || _propertyView.DataContext is not PropertyPageViewModel propview
                    || (textBox?.Parent as FrameworkElement)?.DataContext is not PropertyViewModel prop
                    || propview.Source == null) return;
                doc.CommandBuffer.Execute(PropertyViewServiceBase.GetCommandOfEditingNode(propview.Source, propview.Properties
                    , propview.Properties.IndexOf(prop), textBox.Text));
            });
            ShowEditWindow = new ActionCommand(args => { });
        }

        private void PropertyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            if (tree?.SelectedItem is NodeViewModel selectedVM)
            {
                var list = PropertyViewServiceBase.GetPropertyViewModelOfNode(selectedVM.Source);
                vm.PropertyPage.LoadProperties(list, selectedVM.Source);
            }
        }
    }
}
