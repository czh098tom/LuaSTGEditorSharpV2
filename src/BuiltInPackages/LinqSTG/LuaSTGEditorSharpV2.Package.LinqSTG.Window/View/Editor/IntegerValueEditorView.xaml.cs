using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using ReactiveUI;
using System.Windows;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View.Editor
{
    public partial class IntegerValueEditorView : IViewFor<IntegerValueEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(IntegerValueEditorViewModel), typeof(IntegerValueEditorView), new PropertyMetadata(null));

        public IntegerValueEditorViewModel? ViewModel
        {
            get => (IntegerValueEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IntegerValueEditorViewModel?)value;
        }
        #endregion

        public IntegerValueEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.RawValue, v => v.UpDown.Value)
            ));
        }
    }
}
