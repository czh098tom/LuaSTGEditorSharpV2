using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

        public BlueprintPatternWindow()
        {
            NodeGraphRegistrar.Register();
            InitializeComponent();
            _viewModel = (DataContext as MainViewModel)!;
            Loaded += BlueprintPatternWindow_Loaded;
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

            // Fit the network viewport to the bounding box of all nodes after layout has settled.
            Dispatcher.BeginInvoke(new System.Action(() => NetworkView.CenterAndZoomView()),
                DispatcherPriority.Loaded);
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
