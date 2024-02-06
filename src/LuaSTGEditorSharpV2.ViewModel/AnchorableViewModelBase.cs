using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;

namespace LuaSTGEditorSharpV2.ViewModel
{
    /// <summary>
    /// Base viewmodel for any anchorable pages (excluding document panels)
    /// </summary>
    public class AnchorableViewModelBase : DockingViewModelBase
    {
        public class SelectedNodeChangedEventArgs : EventArgs
        {
            public IDocument? DocumentModel { get; set; }
            public NodeData[] NodeData { get; set; } = [];
        }

        public class PublishCommandEventArgs : EventArgs
        {
            public CommandBase? Command { get; set; }
            public IDocument? DocumentModel { get; set; }
            public NodeData? NodeData { get; set; }
            public bool ShouldRefreshView { get; set; } = true;
        }

        public NodeData[] SourceNodes { get; private set; } = [];

        public IDocument? SourceDocument { get; private set; }

        public event EventHandler<PublishCommandEventArgs>? OnCommandPublishing;

        public override string Title => HostedApplicationHelper.GetService<LocalizationService>()
            ?.GetString(I18NTitleKey, GetType().Assembly) ?? GetType().Name;

        public virtual string I18NTitleKey => GetType().Name;

        public virtual string ContentID => GetType().AssemblyQualifiedName ?? string.Empty;

        public void HandleSelectedNodeChanged(object o, SelectedNodeChangedEventArgs args)
        {
            if (!ShouldChangeSelectedNode(o, args)) return;
            var doc = args.DocumentModel;
            SourceDocument = doc;
            var node = args.NodeData;
            SourceNodes = node;
            HandleSelectedNodeChangedImpl(o, args);
        }

        public virtual bool ShouldChangeSelectedNode(object o, SelectedNodeChangedEventArgs args)
        {
            return true;
        }

        public virtual void HandleSelectedNodeChangedImpl(object o, SelectedNodeChangedEventArgs args)
        {
        }

        protected void PublishCommand(CommandBase? command, IDocument documentModel, NodeData? nodeData, bool shouldRefreshView = true)
        {
            OnCommandPublishing?.Invoke(this, new() 
            {
                Command = command, 
                DocumentModel = documentModel,
                NodeData = nodeData,
                ShouldRefreshView = shouldRefreshView
            });
        }
    }
}
