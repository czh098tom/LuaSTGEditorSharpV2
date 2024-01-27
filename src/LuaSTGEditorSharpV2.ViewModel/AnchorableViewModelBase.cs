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
            public DocumentModel? DocumentModel { get; set; }
            public NodeData? NodeData { get; set; }
        }

        public class PublishCommandEventArgs : EventArgs
        {
            public CommandBase? Command { get; set; }
            public DocumentModel? DocumentModel { get; set; }
            public NodeData? NodeData { get; set; }
            public bool ShouldRefreshView { get; set; } = true;
        }

        public event EventHandler<PublishCommandEventArgs>? OnCommandPublishing;

        public override string Title => HostedApplicationHelper.GetService<LocalizationService>()
            ?.GetString(I18NTitleKey, GetType().Assembly) ?? GetType().Name;

        public virtual string I18NTitleKey => GetType().Name;

        public virtual string ContentID => GetType().AssemblyQualifiedName ?? string.Empty;

        public virtual void HandleSelectedNodeChanged(object o, SelectedNodeChangedEventArgs args)
        {

        }

        protected void PublishCommand(CommandBase? command, DocumentModel documentModel, NodeData? nodeData, bool shouldRefreshView = true)
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
