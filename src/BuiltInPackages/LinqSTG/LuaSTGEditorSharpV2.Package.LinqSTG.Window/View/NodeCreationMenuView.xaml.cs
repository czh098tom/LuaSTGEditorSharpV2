using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View
{
    public partial class NodeCreationMenuView : UserControl
    {
        public NodeCreationMenuView()
        {
            InitializeComponent();
            SearchHint.Text = Localized.linqstg_window_menu_searchHint;
        }

        private NodeCreationMenuViewModel? ViewModel => DataContext as NodeCreationMenuViewModel;

        /// <summary>Moves keyboard focus into the search box; called after the popup opens.</summary>
        public void FocusSearchBox()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchBox.Focus();
                SearchBox.CaretIndex = SearchBox.Text.Length;
            }), DispatcherPriority.Input);
        }

        private void NodeItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is NodeCreationNodeItemViewModel item)
            {
                ViewModel?.RaiseNodeSelected(item);
            }
        }

        private void ResultList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultList.SelectedItem is NodeCreationNodeItemViewModel item)
            {
                ViewModel?.RaiseNodeSelected(item);
            }
        }

        private void ResultList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ResultList.SelectedItem is NodeCreationNodeItemViewModel item)
            {
                ViewModel?.RaiseNodeSelected(item);
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    ViewModel?.RaiseCloseRequested();
                    e.Handled = true;
                    break;
                case Key.Enter when ResultList.Visibility == Visibility.Visible:
                    CommitSelectedResult();
                    e.Handled = true;
                    break;
                case Key.Down when ResultList.Visibility == Visibility.Visible && ResultList.Items.Count > 0:
                    FocusFirstResult();
                    e.Handled = true;
                    break;
            }
        }

        private void CommitSelectedResult()
        {
            var item = ResultList.SelectedItem
                ?? (ResultList.Items.Count > 0 ? ResultList.Items[0] : null);
            if (item is NodeCreationNodeItemViewModel nodeItem)
            {
                ViewModel?.RaiseNodeSelected(nodeItem);
            }
        }

        private void FocusFirstResult()
        {
            ResultList.SelectedIndex = 0;
            ResultList.Focus();
            if (ResultList.ItemContainerGenerator.ContainerFromIndex(0) is ListBoxItem container)
            {
                container.Focus();
            }
        }
    }

    /// <summary>Converts false to <see cref="Visibility.Visible"/> and true to collapsed/hidden.</summary>
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
