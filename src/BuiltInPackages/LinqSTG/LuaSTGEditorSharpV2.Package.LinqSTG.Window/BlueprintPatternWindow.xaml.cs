using System.ComponentModel;
using System.Windows;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public partial class BlueprintPatternWindow : Window
    {
        private MainViewModel _viewModel = null!;

        public BlueprintPatternWindow()
        {
            NodeGraphRegistrar.Register();
            InitializeComponent();
            _viewModel = (DataContext as MainViewModel)!;
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
            _viewModel.Save();
        }
    }
}
