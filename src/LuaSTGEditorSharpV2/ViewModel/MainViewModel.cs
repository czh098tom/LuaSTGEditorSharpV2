using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.Core.Command.Factory;
using LuaSTGEditorSharpV2.Dialog;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class MainViewModel : InjectableViewModel
    {
        private readonly WorkSpaceViewModel _workspace;
        private readonly RibbonViewModel _ribbon;
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

        public MainViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _workspace = serviceProvider.GetRequiredService<WorkSpaceViewModel>();
            _ribbon = serviceProvider.GetRequiredService<RibbonViewModel>();
            _workspace.EnableRequesting += HandleEnableRequesting;
        }

        public void OpenFile(string filePath)
        {
            var activeDocService = ServiceProvider.GetRequiredService<ActiveDocumentService>();
            var doc = activeDocService.Open(filePath);
            if (doc == null) return;
            _workspace.AddDocument(doc);
        }

        public void NewBlankFile()
        {
            var activeDocService = ServiceProvider.GetRequiredService<ActiveDocumentService>();
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
