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
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Xaml.Behaviors.Core;

using Newtonsoft.Json;

using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xceed.Wpf.AvalonDock.Layout;

using Fluent;

using LuaSTGEditorSharpV2.WPF;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Dialog.ViewModel;
using LuaSTGEditorSharpV2.ServiceBridge;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.Services;

using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow, IMainWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = (DataContext as MainViewModel)!;

            var layout = HostedApplicationHelper.GetService<MainWindowLayoutService>();
            layout.LayoutSerializationCallback += HandleLayoutSerializationCallback;
            layout.RefreshSettings();

            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test", "test.lstgxml");

            //vm.OpenFile(testPath);
        }

        private void HandleLayoutSerializationCallback(object? sender, LayoutSerializationCallbackEventArgs e)
        {
            e.Cancel = true;
            if (string.IsNullOrEmpty(e.Model.ContentId)) return;
            var type = Type.GetType(e.Model.ContentId);
            if (type == null) return;
            if (Activator.CreateInstance(type) is not AnchorableViewModelBase anc) return;
            _viewModel.WorkSpace.AddPage(anc);
            e.Content = anc;
            e.Cancel = false;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            if (tree?.SelectedItem is NodeViewModel selectedVM && tree.DataContext is DocumentViewModel dvm)
            {
                _viewModel.WorkSpace.BroadcastSelectedNodeChanged(dvm, selectedVM.Source);
            }
        }

        private void ExecuteOpenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = HostedApplicationHelper.GetService<FileDialogService>();
            foreach (var item in dialog.ShowOpenFileCommandDialog())
            {
                _viewModel.OpenFile(item);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            foreach(var s in HostedApplicationHelper.GetServices<ISettingsSavedOnClose>())
            {
                s.SaveSettings();
            }
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
