using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm;

using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using CommunityToolkit.Mvvm.Input;

namespace LuaSTGEditorSharpV2.Toolbox.ViewModel
{
    public class ToolboxPageViewModel : AnchorableViewModelBase
    {
        public NodeData? SourceNode { get; private set; }

        public DocumentModel? SourceDocument { get; private set; }

        public override string I18NTitleKey => "panel_toolBox_title";

        public ICommand InsertCommand { get; private set; }

        public ToolboxPageViewModel()
        {
            InsertCommand = new RelayCommand(CreateCustomTypeUIDNode);
        }

        public override void HandleSelectedNodeChanged(object o, SelectedNodeChangedEventArgs args)
        {
            var doc = args.DocumentModel;
            if (doc == null) return;
            SourceDocument = doc;
            var node = args.NodeData ?? NodeData.Empty;
            SourceNode = node;
        }

        public void CreateCustomTypeUIDNode()
        {
            if (SourceDocument == null || SourceNode == null) return;
            var windows = new ArrayList(Application.Current.Windows);
            Window? main = null;
            foreach (var win in windows)
            {
                if (win is IMainWindow window)
                {
                    main = window as Window;
                    break;
                }
            }
            InputDialog inputDialog = new() 
            { 
                Owner = main
            };
            if (inputDialog.ShowDialog() == true)
            {
                var type = inputDialog.ViewModel.Text;
                PublishCommand(new AddChildCommand(SourceNode, SourceNode.PhysicalChildren.Count, new NodeData(type)),
                    SourceDocument, SourceNode);
            }
        }
    }
}
