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

using Fluent;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Dialog.ViewModel;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private readonly MainViewModel vm = new();

        private NodeViewModel? selected = null;

        public ICommand CommitEdit { get; private set; }
        public ICommand ShowEditWindow { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            InitializeCommand();

            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test", "test.lstgxml");

            vm.OpenFile(testPath);

            DataContext = vm;
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
            if (tree?.SelectedItem is NodeViewModel selectedVM)
            {
                selected = selectedVM;
                vm.LoadProperties(selectedVM.Source);
                _propertyTab.SelectedIndex = 0;
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (selected == null) return;
            InputDialog inputDialog = new() { Owner = this };
            if (inputDialog.ShowDialog() == true)
            {
                var type = inputDialog.ViewModel.Text;
                vm.InsertNodeOfCustomType(selected.Source, type);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
