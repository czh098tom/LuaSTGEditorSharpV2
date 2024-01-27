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
using LuaSTGEditorSharpV2.ServiceBridge;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.Services;
using System.ComponentModel;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.Diagnostics;
using Xceed.Wpf.AvalonDock.Layout;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow, IMainWindow
    {
        public ICommand CommitEdit { get; private set; }
        public ICommand ShowEditWindow { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            InitializeCommand();

            if (DataContext is not MainViewModel vm) return;

            var layout = HostedApplicationHelper.GetService<MainWindowLayoutService>();
            layout.LayoutSerializationCallback += (o, e) =>
            {
                e.Cancel = true;
                if (string.IsNullOrEmpty(e.Model.ContentId)) return;
                var type = Type.GetType(e.Model.ContentId);
                if (type == null) return;
                if (Activator.CreateInstance(type) is not AnchorableViewModelBase anc) return;
                vm.WorkSpace.AddPage(anc);
                e.Content = anc;
                e.Cancel = false;
            };
            layout.RefreshSettings();

            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test", "test.lstgxml");

            vm.OpenFile(testPath);
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
            if (DataContext is not MainViewModel vm) return;
            var tree = sender as TreeView;
            if (tree?.SelectedItem is NodeViewModel selectedVM && tree.DataContext is DocumentViewModel dvm)
            {
                vm.WorkSpace.BroadcastSelectedNodeChanged(dvm, selectedVM.Source);
                //_propertyTab.SelectedIndex = 0;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            HostedApplicationHelper.GetService<MainWindowLayoutService>().SaveSettings();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog dialog = new() { Owner = this };
            dialog.ShowDialog();
        }
    }
}
