﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace LuaSTGEditorSharpV2.WPF
{
    public class MultiSelectDragableTreeView : MultiSelectTreeView, IDragSource, IDropTarget
    {
        public MultiSelectDragableTreeView() : base()
        {
            DragDrop.SetIsDragSource(this, true);
            DragDrop.SetIsDropTarget(this, true);
            DragDrop.SetDropHandler(this, this);
            DragDrop.SetDragHandler(this, this);
            Style s = new(typeof(MultiSelectTreeViewItem));
            s.Setters.Add(new Setter(DragDrop.IsDragSourceProperty, true));
            s.Setters.Add(new Setter(DragDrop.IsDropTargetProperty, true));
            s.Setters.Add(new Setter(PaddingProperty, new Thickness(0)));
            s.Setters.Add(new Setter(MultiSelectTreeViewItem.IsExpandedProperty, true));
            Resources.Add(typeof(MultiSelectTreeViewItem), s);
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