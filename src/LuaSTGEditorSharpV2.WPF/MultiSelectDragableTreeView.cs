using System;
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
    public class MultiSelectDragableTreeView : MultiSelectTreeView
    {
        public MultiSelectDragableTreeView() : base()
        {
            DragDrop.SetIsDragSource(this, true);
            DragDrop.SetIsDropTarget(this, true);
            DragDrop.SetUseDefaultDragAdorner(this, true);
            Style s = new(typeof(MultiSelectTreeViewItem));
            s.Setters.Add(new Setter(DragDrop.IsDragSourceProperty, true));
            s.Setters.Add(new Setter(DragDrop.IsDropTargetProperty, true));
            s.Setters.Add(new Setter(PaddingProperty, new Thickness(0)));
            s.Setters.Add(new Setter(MultiSelectTreeViewItem.IsExpandedProperty, true));
            Resources.Add(typeof(MultiSelectTreeViewItem), s);
        }
    }
}
