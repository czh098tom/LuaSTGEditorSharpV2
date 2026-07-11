using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using ReactiveUI;
using System.Windows;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View.Editor
{
    public partial class BulletShapeEditorView : IViewFor<BulletShapeEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(BulletShapeEditorViewModel), typeof(BulletShapeEditorView), new PropertyMetadata(null));

        public BulletShapeEditorViewModel? ViewModel
        {
            get => (BulletShapeEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (BulletShapeEditorViewModel?)value;
        }
        #endregion

        public BulletShapeEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.RawValue, v => v.ShapeCombo.SelectedItem)
            ));
        }
    }
}
