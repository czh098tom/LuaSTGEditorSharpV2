using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using DynamicData;
using NodeNetwork.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public partial class BlueprintPatternWindow : Window
    {
        private const double PreviewHalfHeight = 224.0;
        private const double PreviewHalfWidth = 192.0;
        private const double PreviewBuffer = 10.0;

        /// <summary>Drag payload format for variable list entries.</summary>
        private const string VariableDragFormat = "LinqSTG.VariableItem";

        private MainViewModel _viewModel = null!;
        private NodeCreationMenuViewModel? _nodeCreationMenu;
        private Point _pendingNodePosition;
        private PendingConnectionViewModel? _connectionDropPending;
        private int _connectionCountAtDropStart;

        private VariableItemViewModel? _variableDragItem;
        private Point _variableDragStart;

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
            _viewModel.Network.PropertyChanged += Network_PropertyChanged;
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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key != Key.A || Keyboard.Modifiers != ModifierKeys.Shift)
            {
                return;
            }
            if (Keyboard.FocusedElement is TextBoxBase or PasswordBox)
            {
                return;
            }

            var mousePosition = Mouse.GetPosition(NetworkView);
            if (mousePosition.X < 0 || mousePosition.Y < 0
                || mousePosition.X > NetworkView.ActualWidth
                || mousePosition.Y > NetworkView.ActualHeight)
            {
                return;
            }

            _pendingNodePosition = ScreenToNetwork(mousePosition);
            OpenNodeCreationPopup(mousePosition);
            e.Handled = true;
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

        private Point NetworkToScreen(Point position)
        {
            var region = NetworkView.NetworkViewportRegion;
            double scaleX = region.Width > 0 ? NetworkView.ActualWidth / region.Width : 1.0;
            double scaleY = region.Height > 0 ? NetworkView.ActualHeight / region.Height : 1.0;
            return new Point((position.X - region.X) * scaleX, (position.Y - region.Y) * scaleY);
        }

        private void Network_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(NetworkViewModel.PendingConnection)) return;

            var pending = _viewModel.Network.PendingConnection;
            if (pending is not null)
            {
                _connectionDropPending = pending;
                _connectionCountAtDropStart = _viewModel.Network.Connections.Count;
                return;
            }

            var dropped = _connectionDropPending;
            _connectionDropPending = null;
            if (dropped is null
                || _viewModel.Network.Connections.Count != _connectionCountAtDropStart
                || (dropped.Input is not null && dropped.Output is not null))
            {
                return;
            }

            ShowCompatibleNodeMenu(dropped);
        }

        private void ShowCompatibleNodeMenu(PendingConnectionViewModel pending)
        {
            var entries = GetCompatibleNodeEntries(pending).ToList();
            if (entries.Count == 0) return;

            var position = NetworkToScreen(pending.LooseEndPoint);
            var menu = new ContextMenu
            {
                PlacementTarget = NetworkView,
                Placement = PlacementMode.Relative,
                HorizontalOffset = position.X,
                VerticalOffset = position.Y,
            };

            foreach (var entry in entries)
            {
                var menuItem = new MenuItem { Header = entry.Title };
                menuItem.Click += (sender, args) => AddCompatibleNode(entry, pending);
                menu.Items.Add(menuItem);
            }

            menu.IsOpen = true;
        }

        private static IEnumerable<NodeCreationEntry> GetCompatibleNodeEntries(PendingConnectionViewModel pending)
        {
            foreach (var entry in NodeCreationCatalog.GetEntries())
            {
                var node = entry.CreateNode();
                var compatible = pending.Output is { } output
                    ? node.Inputs.Items.Any(input => CanConnect(output, input))
                    : pending.Input is { } input && node.Outputs.Items.Any(output => CanConnect(output, input));
                if (compatible)
                {
                    yield return entry;
                }
            }
        }

        private static bool CanConnect(NodeOutputViewModel output, NodeInputViewModel input)
        {
            if (input.Port is null || output.Port is null || input.Connections.Count != 0)
            {
                return false;
            }
            if (input is ContextAwareNodeInputViewModel)
            {
                return true;
            }

            var expectedType = GetExpectedInputType(input);
            var outputType = GetOutputValueType(output);
            return expectedType is not null && outputType is not null && expectedType == outputType;
        }

        private static Type? GetExpectedInputType(NodeInputViewModel input)
        {
            var type = input.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LinqSTGNodeInputViewModel<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        private static Type? GetOutputValueType(NodeOutputViewModel output)
        {
            if (output is ContextAwareNodeOutputViewModel contextAware)
            {
                return contextAware.CurrentType;
            }

            var type = output.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LinqSTGNodeOutputViewModel<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        private void AddCompatibleNode(NodeCreationEntry entry, PendingConnectionViewModel pending)
        {
            var node = entry.CreateNode();
            node.Position = pending.LooseEndPoint;
            _viewModel.Network.Nodes.Add(node);

            if (pending.Output is { } sourceOutput)
            {
                var targetInput = node.Inputs.Items.FirstOrDefault(input => CanConnect(sourceOutput, input));
                if (targetInput is not null)
                {
                    _viewModel.Network.Connections.Add(
                        new LinqSTGConnectionViewModel(_viewModel.Network, targetInput, sourceOutput));
                }
            }
            else if (pending.Input is { } sourceInput)
            {
                var targetOutput = node.Outputs.Items.FirstOrDefault(output => CanConnect(output, sourceInput));
                if (targetOutput is not null)
                {
                    _viewModel.Network.Connections.Add(
                        new LinqSTGConnectionViewModel(_viewModel.Network, sourceInput, targetOutput));
                }
            }
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

        #region Variable list

        private void AddVariableButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.VariableList.AddItem();
        }

        private void RemoveVariableButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: VariableItemViewModel item })
            {
                _viewModel.VariableList.RemoveItem(item);
            }
        }

        /// <summary>
        /// Commits name/value edits on Enter (the text boxes otherwise commit on
        /// LostFocus) and reverts the edit on Escape.
        /// </summary>
        private void VariableTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            if (e.Key == Key.Enter)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                e.Handled = true;
            }
        }

        private void VariableGrip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: VariableItemViewModel item })
            {
                _variableDragItem = item;
                _variableDragStart = e.GetPosition(null);
            }
        }

        private void VariableGrip_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_variableDragItem is null)
            {
                return;
            }
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _variableDragItem = null;
                return;
            }

            var position = e.GetPosition(null);
            if (Math.Abs(position.X - _variableDragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(position.Y - _variableDragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            var item = _variableDragItem;
            _variableDragItem = null;
            // Locked entries cannot be reordered inside the list, but they may
            // still be dropped into the blueprint area.
            var data = new DataObject(VariableDragFormat, item);
            DragDrop.DoDragDrop((FrameworkElement)sender, data, DragDropEffects.Move | DragDropEffects.Copy);
        }

        private void VariableItemsControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(VariableDragFormat) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void VariableItemsControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(VariableDragFormat) is not VariableItemViewModel item)
            {
                return;
            }
            e.Handled = true;
            var items = _viewModel.VariableList.Items;
            _viewModel.VariableList.MoveItem(items.IndexOf(item), ComputeVariableDropIndex(e));
        }

        /// <summary>
        /// Index at which a dropped entry should be inserted: before the row whose
        /// vertical midpoint is below the drop position, or at the end.
        /// </summary>
        private int ComputeVariableDropIndex(DragEventArgs e)
        {
            var position = e.GetPosition(VariableItemsControl);
            var items = _viewModel.VariableList.Items;
            var generator = VariableItemsControl.ItemContainerGenerator;
            for (int i = 0; i < items.Count; i++)
            {
                if (generator.ContainerFromIndex(i) is not FrameworkElement container) continue;
                var top = container.TranslatePoint(new Point(0, 0), VariableItemsControl).Y;
                if (position.Y < top + container.ActualHeight / 2) return i;
            }
            return items.Count;
        }

        private void NetworkView_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(VariableDragFormat) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void NetworkView_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(VariableDragFormat) is not VariableItemViewModel item)
            {
                return;
            }
            e.Handled = true;
            var position = ScreenToNetwork(e.GetPosition(NetworkView));
            // Locked built-ins generate their dedicated node types; the entry's
            // value type picks the generic node variant otherwise.
            _viewModel.AddNodeForVariable(item, position);
        }

        #endregion
    }
}
