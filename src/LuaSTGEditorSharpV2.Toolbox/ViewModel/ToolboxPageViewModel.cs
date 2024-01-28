using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.WPF;
using LuaSTGEditorSharpV2.Core.Command.Service;

namespace LuaSTGEditorSharpV2.Toolbox.ViewModel
{
    public class ToolboxPageViewModel : AnchorableViewModelBase
    {
        public override string I18NTitleKey => "panel_toolBox_title";

        public ICommand InsertCommand { get; private set; }

        public ToolboxPageViewModel()
        {
            InsertCommand = new RelayCommand(CreateCustomTypeUIDNode);
        }

        public override void HandleSelectedNodeChangedImpl(object o, SelectedNodeChangedEventArgs args)
        {
        }

        public void CreateCustomTypeUIDNode()
        {
            if (SourceDocument == null || SourceNode == null) return;
            InputDialog inputDialog = new()
            {
                Owner = WindowHelper.GetMainWindow()
            };
            if (inputDialog.ShowDialog() == true)
            {
                var type = inputDialog.ViewModel.Text;
                var command = HostedApplicationHelper.GetService<InsertCommandHostingService>().InsertCommandFactory
                    .CreateInsertCommand(SourceNode, new NodeData(type));
                PublishCommand(command, SourceDocument, SourceNode);
            }
        }
    }
}
