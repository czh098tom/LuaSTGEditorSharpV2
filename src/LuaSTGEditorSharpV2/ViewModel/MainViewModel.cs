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
using LuaSTGEditorSharpV2.Core.Command.Factory;
using LuaSTGEditorSharpV2.Dialog;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly WorkSpaceViewModel _workspace = new();
        private readonly RibbonViewModel _ribbon = new();
        private readonly StatusBarViewModel _statusBar = new();

        public WorkSpaceViewModel WorkSpace
        {
            get => _workspace;
        }

        public RibbonViewModel Ribbon
        {
            get => _ribbon;
        }

        public StatusBarViewModel StatusBar
        {
            get => _statusBar;
        }

        public MainViewModel()
        {
            _workspace.EnableRequesting += HandleEnableRequesting;
        }

        public void OpenFile(string filePath)
        {
            var activeDocService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var doc = activeDocService.Open(filePath);
            if (doc == null) return;
            _workspace.AddDocument(doc);
        }

        public void NewBlankFile()
        {
            var activeDocService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var doc = activeDocService.CreateBlank();
            if (doc == null) return;
            _workspace.AddDocument(doc);
        }

        private void HandleEnableRequesting(object? sender, OnEnableHandleRequestedEventArgs e)
        {
            e.Add(_workspace.IsEnabledHandle.RequestNonNormalState());
            e.Add(_ribbon.IsEnabledHandle.RequestNonNormalState());
        }
    }
}
