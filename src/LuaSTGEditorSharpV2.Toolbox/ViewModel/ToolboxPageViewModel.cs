using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.WPF;
using LuaSTGEditorSharpV2.Core.Command.Service;
using LuaSTGEditorSharpV2.Toolbox.Service;
using System.Drawing.Design;

namespace LuaSTGEditorSharpV2.Toolbox.ViewModel
{
    public class ToolboxPageViewModel : AnchorableViewModelBase
    {
        public override string I18NTitleKey => "panel_toolBox_title";

        private ObservableCollection<ToolboxItemViewModel> _toolboxItems = [];
        public ObservableCollection<ToolboxItemViewModel> ToolboxItems
        {
            get => _toolboxItems;
            set
            {
                _toolboxItems = value;
                RaisePropertyChanged();
            }
        }

        public ToolboxPageViewModel()
        {
            LoadFromLoadedPackage();
        }

        public void LoadFromLoadedPackage()
        {
            _toolboxItems.Clear();
            var service = HostedApplicationHelper.GetService<ToolboxProviderService>();
            var list = service.CreateTree();
            for (int i = 0; i < list.Count; i++)
            {
                _toolboxItems.Add(list[i]);
            }
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            Stack<ToolboxItemViewModel> q = [];
            foreach (var item in _toolboxItems)
            {
                q.Push(item);
            }
            while (q.Count > 0)
            {
                var n = q.Pop();
                foreach (var c in n.Children)
                {
                    q.Push(c);
                }
                n.NodeCreated += CreateNode;
                n.NodeCanBeCreated += NodeCanBeCreated;
            }
        }

        private void NodeCanBeCreated(object? sender, ToolboxItemViewModel.NodeCreateEventArgs e)
        {
            e.CanCreate = SourceDocument != null && SourceNodes.Length > 0;
        }

        private void CreateNode(object? sender, ToolboxItemViewModel.NodeCreateEventArgs e)
        {
            if (SourceDocument == null) return;
            if (e.CreatedData.Length > 0)
            {
                var insCommandHost = HostedApplicationHelper.GetService<InsertCommandHostingService>();
                PublishCommand(SourceNodes.SelectCommand(ori => e.CreatedData
                    .SelectCommand(toIns => insCommandHost.InsertCommandFactory.CreateInsertCommand(ori, toIns)))
                    , SourceDocument, SourceNodes);
            }
        }

        private void CreateCustomTypeUIDNode()
        {
            if (SourceDocument == null) return;
            InputDialog inputDialog = new()
            {
                Owner = WindowHelper.GetMainWindow()
            };
            if (inputDialog.ShowDialog() == true)
            {
                var insCommandHost = HostedApplicationHelper.GetService<InsertCommandHostingService>();
                PublishCommand(SourceNodes.SelectCommand(n => insCommandHost.InsertCommandFactory
                    .CreateInsertCommand(n, new NodeData(inputDialog.ViewModel.Text))), SourceDocument, SourceNodes);
            }
        }
    }
}
