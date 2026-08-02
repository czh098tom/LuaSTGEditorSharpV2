using System.ComponentModel;
using System.Windows;
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

            // Fit the network viewport to the bounding box of all nodes after layout has settled.
            Dispatcher.BeginInvoke(new System.Action(() => NetworkView.CenterAndZoomView()),
                DispatcherPriority.Loaded);
        }
    }
}
