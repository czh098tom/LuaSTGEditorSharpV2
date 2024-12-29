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

namespace LuaSTGEditorSharpV2.View
{
    /// <summary>
    /// DocumentTreeView.xaml 的交互逻辑
    /// </summary>
    public partial class DocumentTreeView : TreeView
    {
        /// <summary>
        /// 
        /// </summary>
        /// <see cref="https://blog.csdn.net/lishuangquan1987/article/details/115305335"/>
        public static new readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached(nameof(SelectedItem), typeof(object), typeof(DocumentTreeView), 
                new UIPropertyMetadata(null, HandleSelectedItemChanged));

        private static void HandleSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is not TreeView treeView)
                return;

            if (e.NewValue == treeView.SelectedItem) return;

            ChangeSelectedItem(treeView, e.NewValue);
        }

        private static void ChangeSelectedItem(TreeView treeView, object p)
        {
            var item = FindItemByDataContext(treeView, p);
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        private static TreeViewItem? FindItemByDataContext(TreeView treeView, object dataContext)
        {
            for (int i = 0; i < treeView.Items.Count; i++)
            {
                if (treeView.ItemContainerGenerator.ContainerFromIndex(i) is not TreeViewItem treeItem) continue;

                var result = FindItemByDataContext(treeItem, dataContext);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private static TreeViewItem? FindItemByDataContext(TreeViewItem item, object dataContext)
        {
            if (item.DataContext == dataContext)
            {
                return item;
            }

            for (int i = 0; i < item.Items.Count; i++)
            {
                if (item.ItemContainerGenerator.ContainerFromIndex(i) is not TreeViewItem subItem) continue;

                var result = FindItemByDataContext(subItem, dataContext);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public new object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public DocumentTreeView()
        {
            InitializeComponent();
            SelectedItemChanged += DocumentTreeView_SelectedItemChanged;
        }

        private void DocumentTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }
    }
}
