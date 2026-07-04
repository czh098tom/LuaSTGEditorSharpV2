using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using ReactiveUI;
using System.Windows;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View.Editor
{
    public partial class IntervalTypeEditorView : IViewFor<IntervalTypeEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(IntervalTypeEditorViewModel), typeof(IntervalTypeEditorView), new PropertyMetadata(null));

        public IntervalTypeEditorViewModel? ViewModel
        {
            get => (IntervalTypeEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IntervalTypeEditorViewModel?)value;
        }
        #endregion

        public IntervalTypeEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.HeadClosed, v => v.HeadToggle.IsChecked));
                d(this.Bind(ViewModel, vm => vm.TailClosed, v => v.TailToggle.IsChecked));
            });
        }
    }
}
