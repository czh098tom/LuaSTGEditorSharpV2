using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using PropertyTools.Wpf;
using PropertyTools;

namespace LuaSTGEditorSharpV2.WPF
{
    public class TreeViewEx : TreeListBox
    {
        /// <summary>
        /// 
        /// </summary>
        /// <see cref="https://blog.csdn.net/lishuangquan1987/article/details/115305335"/>
        public static readonly DependencyProperty SelectedItemExProperty =
            DependencyProperty.RegisterAttached(
                nameof(SelectedItemEx), 
                typeof(IEnumerable), 
                typeof(TreeViewEx),
                new UIPropertyMetadata(null, HandleSelectedItemChanged));

        private static void HandleSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is not TreeListBox treeView || e.NewValue is not IEnumerable enumerable)
            {
                return;
            }

            ChangeSelectedItem(treeView, enumerable);
        }

        private static void ChangeSelectedItem(TreeListBox treeView, IEnumerable p)
        {
            HashSet<object> set = [];

            foreach (var item in p)
            {
                set.Add(item);
            }

            for (int i = 0; i < treeView.Items.Count; i++)
            {
                if (treeView.ItemContainerGenerator.ContainerFromIndex(i) is not TreeListBoxItem treeItem) continue;

                if (set.Contains(treeItem.DataContext))
                {
                    treeItem.IsSelected = true;
                }
            }
        }

        public IEnumerable? SelectedItemEx
        {
            get => GetValue(SelectedItemExProperty) as IEnumerable;
            set => SetValue(SelectedItemExProperty, value);
        }

        public TreeViewEx() : base()
        {
            SelectionChanged += TreeViewEx_SelectionChanged;
        }

        private void TreeViewEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemEx = SelectedItems;
        }
    }
}
