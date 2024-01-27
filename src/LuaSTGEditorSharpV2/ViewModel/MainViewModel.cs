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

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly WorkSpaceViewModel _workspace = new();

        public WorkSpaceViewModel WorkSpace
        {
            get => _workspace;
        }

        public void OpenFile(string filePath)
        {
            var activeDocService = HostedApplicationHelper.GetService<ActiveDocumentService>();
            var doc = activeDocService.Open(filePath);
            if (doc == null) return;
            _workspace.AddDocument(doc);
        }
    }
}
