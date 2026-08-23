using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public partial class BlueprintPatternWindow : Window
    {
        private const double PreviewHalfHeight = 224.0;
        private const double PreviewHalfWidth = 192.0;
        private const double PreviewBuffer = 10.0;

        private MainViewModel _viewModel = null!;
        private NodeCreationMenuViewModel? _nodeCreationMenu;
        private Point _pendingNodePosition;

        public BlueprintPatternWindow()
        {
            NodeGraphRegistrar.Register();
            InitializeComponent();
            _viewModel = (DataContext as MainViewModel)!;
            Loaded += BlueprintPatternWindow_Loaded;

            // Free the right mouse button for the node creation menu; cut moves to Ctrl+RightClick.
            NetworkView.StartCutGesture = new MouseGesture(MouseAction.RightClick, ModifierKeys.Control);
            NetworkView.PreviewMouseRightButtonUp += NetworkView_PreviewMouseRightButtonUp;
        }

        public string? NetworkJson
        {
            get => _viewModel.NetworkJson;
            set
            {
                _viewModel.NetworkJson = value;
                _viewModel.Load();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _viewModel.Pause();
            _viewModel.Save();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsPlaying)
            {
                _viewModel.Pause();
            }
            else
            {
                _viewModel.Play();
            }
        }

        private void BlueprintPatternWindow_Loaded(object sender, RoutedEventArgs e)
        {
            double width = PreviewHost.ActualWidth;
            double previewHeight = PreviewArea.ActualHeight;
            if (width <= 0 || previewHeight <= 0) return;

            double scale = width / (2.0 * (PreviewHalfWidth + PreviewBuffer));
            PreviewCanvas.Scale = scale;
            PreviewCanvas.TranslateOffset = new Point(width / 2.0, previewHeight / 2.0);

            // Localize the hardcoded "Search..." watermark inside the third-party NodeListView.
            if (FindDescendantByName(NodeListView, "emptySearchBoxMessage") is TextBlock watermark)
            {
                watermark.Text = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_nodeList_searchHint;
            }

            if (_nodeCreationMenu is null && NodeCreationMenu.DataContext is NodeCreationMenuViewModel menuViewModel)
            {
                _nodeCreationMenu = menuViewModel;
                menuViewModel.NodeSelected += NodeCreationMenu_NodeSelected;
                menuViewModel.CloseRequested += (_, _) => NodeCreationPopup.IsOpen = false;
            }

            // Fit the network viewport to the bounding box of all nodes after layout has settled.
            Dispatcher.BeginInvoke(new System.Action(() => NetworkView.CenterAndZoomView()),
                DispatcherPriority.Loaded);
        }

        private void NetworkView_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Releasing Ctrl+RightClick ends the cut-connections drag; the release must
            // finish the cut, not open the creation menu. Leave the event unhandled so
            // NodeNetwork's own cut handling completes.
            var cutGesture = NetworkView.StartCutGesture;
            if (cutGesture.Modifiers != ModifierKeys.None &&
                Keyboard.Modifiers == cutGesture.Modifiers)
            {
                return;
            }
            if (_viewModel.Network.CutLine.IsVisible)
            {
                return;
            }

            e.Handled = true;
            var screenPosition = e.GetPosition(NetworkView);
            _pendingNodePosition = ScreenToNetwork(screenPosition);

            // Open after the current input event has fully completed: a StaysOpen=False
            // popup opened while the mouse press is still in flight dismisses itself as
            // soon as the button is released outside of it.
            Dispatcher.BeginInvoke(new Action(() => OpenNodeCreationPopup(screenPosition)),
                DispatcherPriority.Input);
        }

        private void OpenNodeCreationPopup(Point screenPosition)
        {
            _nodeCreationMenu ??= NodeCreationMenu.DataContext as NodeCreationMenuViewModel;
            _nodeCreationMenu?.Reload();
            _nodeCreationMenu!.SearchText = string.Empty;

            NodeCreationPopup.HorizontalOffset = 0;
            NodeCreationPopup.VerticalOffset = 0;
            NodeCreationPopup.IsOpen = true;
            NodeCreationMenu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            PlaceNodeCreationPopup(screenPosition, NodeCreationMenu.DesiredSize);
            NodeCreationMenu.FocusSearchBox();
        }

        /// <summary>
        /// Maps a NetworkView-relative screen point into network (node Position) coordinates
        /// by linear interpolation over <see cref="NodeNetwork.Views.NetworkView.NetworkViewportRegion"/>,
        /// which is the world-space rectangle covered by the view.
        /// </summary>
        private Point ScreenToNetwork(Point position)
        {
            var region = NetworkView.NetworkViewportRegion;
            double scaleX = NetworkView.ActualWidth > 0 ? region.Width / NetworkView.ActualWidth : 1.0;
            double scaleY = NetworkView.ActualHeight > 0 ? region.Height / NetworkView.ActualHeight : 1.0;
            return new Point(region.X + position.X * scaleX, region.Y + position.Y * scaleY);
        }

        private void PlaceNodeCreationPopup(Point clickPosition, Size menuSize)
        {
            double x = clickPosition.X + 2;
            double y = clickPosition.Y + 2;
            if (x + menuSize.Width > NetworkView.ActualWidth)
            {
                x = Math.Max(0, clickPosition.X - menuSize.Width - 2);
            }
            if (y + menuSize.Height > NetworkView.ActualHeight)
            {
                y = Math.Max(0, clickPosition.Y - menuSize.Height - 2);
            }
            NodeCreationPopup.HorizontalOffset = x;
            NodeCreationPopup.VerticalOffset = y;
        }

        private void NodeCreationMenu_NodeSelected(NodeCreationNodeItemViewModel item)
        {
            NodeCreationPopup.IsOpen = false;
            _viewModel.AddNode(item.Entry, _pendingNodePosition);
        }

        private static DependencyObject? FindDescendantByName(DependencyObject root, string name)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is FrameworkElement element && element.Name == name)
                {
                    return element;
                }
                var found = FindDescendantByName(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
