using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
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

        /// <summary>
        /// Defers attaching the IsExpanded binding until container generation has finished.
        /// <para>
        /// TreeListBox attaches the IsExpanded binding inside PrepareContainerForItemOverride,
        /// which runs while the ItemContainerGenerator is generating containers - i.e. in the
        /// middle of VirtualizingStackPanel's measure pass while scrolling. The pushed value
        /// invokes TreeListBoxItem.IsExpandedChanged -> TreeListBox.Expand -> Items.Insert,
        /// mutating the item collection from inside the panel's layout. On large documents
        /// this crashes WPF virtualization with "Height must be non-negative"
        /// (PropertyTools #38/#142/#165; on zh-CN systems PropertyTools 3.1.0 fails to
        /// recognize the localized message and rethrows) or with a stack overflow in
        /// VirtualizingStackPanel.MeasureOverrideImpl. Attaching the identical binding one
        /// dispatcher turn later keeps expansion lazy without touching layout mid-pass.
        /// </para>
        /// </summary>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var path = IsExpandedPath;
            if (string.IsNullOrEmpty(path) || element is not TreeListBoxItem container)
            {
                base.PrepareContainerForItemOverride(element, item);
                return;
            }

            // Hide the path so the base class skips its synchronous SetBinding; restore it
            // immediately afterwards (both use the same dependency property instance).
            SetCurrentValue(IsExpandedPathProperty, string.Empty);
            try
            {
                base.PrepareContainerForItemOverride(element, item);
            }
            finally
            {
                SetCurrentValue(IsExpandedPathProperty, path);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.DataBind,
                static (TreeListBoxItem c, string p) =>
                    c.SetBinding(TreeListBoxItem.IsExpandedProperty, new Binding(p)),
                container, path);
        }

        private void TreeViewEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemEx = SelectedItems;
        }
    }
}
