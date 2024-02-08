using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GongSolutions.Wpf.DragDrop;

namespace LuaSTGEditorSharpV2.View
{
    /// <summary>
    /// DocumentTreeView.xaml 的交互逻辑
    /// </summary>
    public partial class DocumentTreeView : TreeView, IDropTarget
    {
        public class DropEventArgs(IDropInfo dropInfo) : EventArgs
        {
            public IDropInfo DropInfo { get; set; } = dropInfo;
        }

        public event EventHandler<DropEventArgs>? Dropping;
        public event EventHandler<DropEventArgs>? DraggingOver;

        public DocumentTreeView()
        {
            InitializeComponent();
            GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(this, this);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            DraggingOver?.Invoke(this, new DropEventArgs(dropInfo));
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Move;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            Dropping?.Invoke(this, new DropEventArgs(dropInfo));
        }
    }
}
