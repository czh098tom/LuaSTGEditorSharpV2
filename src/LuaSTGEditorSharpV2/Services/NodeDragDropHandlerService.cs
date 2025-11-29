using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Editor.Extension;

namespace LuaSTGEditorSharpV2.Services
{
    [Inject(ServiceLifetime.Scoped, typeof(IDragDropHandler), key: ScopeKey.EditorNode)]
    public class NodeDragDropHandlerService(
        [FromKeyedServices(ScopeKey.EditorNode)] EditorNode editorNode) : IDragDropHandler
    {
        private readonly Lazy<NodeViewModel> _viewModel = new(editorNode.GetRequiredNodeService<NodeViewModel>);

        public void Detach()
        {
        }

        public bool CanDrop(EditorNode editorNode, DropRelativePosition position, DragDropOperation operation)
        {
            return position == DropRelativePosition.Child || editorNode.Parent != null;
        }

        public void Drop(IEnumerable<EditorNode> items, DropRelativePosition position, DragDropOperation operation)
        {
            var expandedWithChildren = editorNode.Children.Count > 0 && _viewModel.Value.IsExpanded;
            var command = items.SelectFilter(en => (position, operation, expandedWithChildren) switch
            {
                (DropRelativePosition.Child, DragDropOperation.Copy, _) => CheckedCommand.InsertNodeAsLastChild(editorNode, en.Source),
                (DropRelativePosition.Child, DragDropOperation.Move, _) => CheckedCommand.MoveAsLastChild(editorNode, en),
                (DropRelativePosition.Before, DragDropOperation.Copy, _) => CheckedCommand.InsertNodeBefore(editorNode, en.Source),
                (DropRelativePosition.Before, DragDropOperation.Move, _) => CheckedCommand.MoveToBefore(editorNode, en),
                (DropRelativePosition.After, DragDropOperation.Copy, false) => CheckedCommand.InsertNodeAfter(editorNode, en.Source),
                (DropRelativePosition.After, DragDropOperation.Move, false) => CheckedCommand.MoveToAfter(editorNode, en),
                (DropRelativePosition.After, DragDropOperation.Copy, true) => CheckedCommand.InsertNodeAsFirstChild(editorNode, en.Source),
                (DropRelativePosition.After, DragDropOperation.Move, true) => CheckedCommand.MoveAsFirstChild(editorNode, en),
                _ => null
            });
            if (command != null)
            {
                editorNode.Document.ExecuteCommand(command);
            }
        }
    }
}
