using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            // Keep the content of both areas centered while the splitter or the window resizes them.
            PreviewCanvas.SizeChanged += PreviewCanvas_SizeChanged;
            NetworkView.SizeChanged += NetworkView_SizeChanged;
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

        /// <summary>
        /// On the initial layout the play field (centered on the world origin) is fitted to the
        /// viewport width and centered. On later resizes the scale is kept and the world point
        /// that was at the viewport center before the resize stays centered.
        /// </summary>
        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0) return;

            if (e.PreviousSize.Width <= 0 || e.PreviousSize.Height <= 0)
            {
                PreviewCanvas.Scale = e.NewSize.Width / (2.0 * (PreviewHalfWidth + PreviewBuffer));
                PreviewCanvas.TranslateOffset = new Point(e.NewSize.Width / 2.0, e.NewSize.Height / 2.0);
                return;
            }

            var offset = PreviewCanvas.TranslateOffset;
            PreviewCanvas.TranslateOffset = new Point(
                offset.X + (e.NewSize.Width - e.PreviousSize.Width) / 2.0,
                offset.Y + (e.NewSize.Height - e.PreviousSize.Height) / 2.0);
        }

        /// <summary>
        /// Keeps the world point under the view center fixed while the network area is resized:
        /// the view center moves by half the size delta in screen pixels, so the content is
        /// shifted by the same amount via the drag offset.
        /// </summary>
        private void NetworkView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width <= 0 || e.PreviousSize.Height <= 0) return;

            var network = _viewModel.Network;
            network.DragOffset = new Point(
                network.DragOffset.X + (e.NewSize.Width - e.PreviousSize.Width) / 2.0,
                network.DragOffset.Y + (e.NewSize.Height - e.PreviousSize.Height) / 2.0);
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
    }
}
