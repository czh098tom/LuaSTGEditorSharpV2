using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using GongSolutions.Wpf.DragDrop;

using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace LuaSTGEditorSharpV2.WPF
{
    public class MultiselectDraggableTreeViewItem : MultiSelectTreeViewItem, IDragSource, IDropTarget
    {
        public MultiselectDraggableTreeViewItem() : base()
        {
            DragDrop.SetDropHandler(this, this);
            DragDrop.SetDragHandler(this, this);
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return DragDrop.DefaultDragHandler.CanStartDrag(dragInfo);
        }

        public void DragCancelled()
        {
            DragDrop.DefaultDragHandler.DragCancelled();
        }

        public void DragDropOperationFinished(System.Windows.DragDropEffects operationResult, IDragInfo dragInfo)
        {
            DragDrop.DefaultDragHandler.DragDropOperationFinished(operationResult, dragInfo);
        }

        public void Dropped(IDropInfo dropInfo)
        {
            DragDrop.DefaultDragHandler.Dropped(dropInfo);
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            DragDrop.DefaultDragHandler.StartDrag(dragInfo);
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return DragDrop.DefaultDragHandler.TryCatchOccurredException(exception);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.DragOver(dropInfo);
            var pos = dropInfo.DropPosition;
            var s = dropInfo.VisualTargetItem.RenderSize;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }
}
