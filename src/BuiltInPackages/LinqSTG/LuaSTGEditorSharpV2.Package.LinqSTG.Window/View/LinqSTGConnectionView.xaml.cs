using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View
{
    /// <summary>
    /// LinqSTGConnectionView.xaml 的交互逻辑
    /// </summary>
    public partial class LinqSTGConnectionView : IViewFor<LinqSTGConnectionViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(LinqSTGConnectionViewModel), typeof(LinqSTGConnectionView), new PropertyMetadata(null));

        public LinqSTGConnectionViewModel? ViewModel
        {
            get => (LinqSTGConnectionViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (LinqSTGConnectionViewModel?)value;
        }

        public LinqSTGConnectionView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.ConnectionView.ViewModel).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.ColorInput, 
                    v => v.ConnectionView.RegularBrush, color => new SolidColorBrush(color));
            });
        }
    }
}
