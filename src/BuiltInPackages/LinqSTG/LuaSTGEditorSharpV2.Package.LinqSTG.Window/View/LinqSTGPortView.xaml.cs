using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
    /// LinqSTGPortView.xaml 的交互逻辑
    /// </summary>
    public partial class LinqSTGPortView : IViewFor<LinqSTGPortViewModel>
    {
        public static readonly Color DefaultPortColor = Color.FromArgb(0xFF, 0x6B, 0xA0, 0xC7);

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(LinqSTGPortViewModel), typeof(LinqSTGPortView), new PropertyMetadata(null));

        public static readonly DependencyProperty PortColorProperty =
            DependencyProperty.Register(nameof(PortColor), typeof(Color), typeof(LinqSTGPortView), new PropertyMetadata(DefaultPortColor));

        public LinqSTGPortViewModel? ViewModel
        {
            get => (LinqSTGPortViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public Color PortColor
        {
            get => (Color)GetValue(PortColorProperty);
            set => SetValue(PortColorProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (LinqSTGPortViewModel?)value;
        }

        public LinqSTGPortView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.PortView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PortColor, 
                    v => v.PortView.RegularStroke, c => new SolidColorBrush(c)).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PortColor,
                    v => v.PortView.ConnectedStroke, c => new SolidColorBrush(c)).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PortColor,
                    v => v.PortView.ConnectedFill, c => new SolidColorBrush(c)).DisposeWith(d);
            });
        }
    }
}
