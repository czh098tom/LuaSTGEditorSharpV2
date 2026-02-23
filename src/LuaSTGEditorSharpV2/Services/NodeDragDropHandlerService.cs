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
    using static CheckedCommand;

    [Inject(ServiceLifetime.Scoped, typeof(IDragDropHandler), key: ScopeKey.EditorNode)]
    public class NodeDragDropHandlerService(
        [FromKeyedServices(ScopeKey.EditorNode)] EditorNode self) : IDragDropHandler
    {
        private readonly Lazy<NodeViewModel> _viewModel = new(self.GetRequiredNodeService<NodeViewModel>);

        public void Detach()
        {
        }

        public bool CanDrop(EditorNode editorNode, DropRelativePosition position, DragDropOperation operation)
        {
            return editorNode.Document == self.Document 
                && (position == DropRelativePosition.Child || editorNode.Parent != null);
        }

        public void Drop(IEnumerable<EditorNode> items, DropRelativePosition position, DragDropOperation operation)
        {
            items = self.Document.OrderByViewOrder(items);
            var expandedWithChildren = self.Children.Count > 0 && _viewModel.Value.IsExpanded;
            var command = (position, operation, expandedWithChildren) switch
            {
                (DropRelativePosition.Child, DragDropOperation.Copy, _) => InsertNode.Many.AsLastChild(self.Document, self.GetPath(), items.Select(en => en.Source)),
                (DropRelativePosition.Child, DragDropOperation.Move, _) => MoveNode.Many.AsLastChild(self.Document, self.GetPath(), items),
                (DropRelativePosition.Before, DragDropOperation.Copy, _) => InsertNode.Many.ToBefore(self.Document, self.GetPath(), items.Select(en => en.Source)),
                (DropRelativePosition.Before, DragDropOperation.Move, _) => MoveNode.Many.ToBefore(self.Document, self.GetPath(), items),
                (DropRelativePosition.After, DragDropOperation.Copy, false) => InsertNode.Many.ToAfter(self.Document, self.GetPath(), items.Select(en => en.Source)),
                (DropRelativePosition.After, DragDropOperation.Move, false) => MoveNode.Many.ToAfter(self.Document, self.GetPath(), items),
                (DropRelativePosition.After, DragDropOperation.Copy, true) => InsertNode.Many.AsFirstChild(self.Document, self.GetPath(), items.Select(en => en.Source)),
                (DropRelativePosition.After, DragDropOperation.Move, true) => MoveNode.Many.AsFirstChild(self.Document, self.GetPath(), items),
                _ => null
            };
            if (command != null)
            {
                self.Document.ExecuteCommand(command);
            }
        }
    }
}
