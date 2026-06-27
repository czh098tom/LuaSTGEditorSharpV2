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
    /// LinqSTGNodeView.xaml 的交互逻辑
    /// </summary>
    public partial class LinqSTGNodeView : IViewFor<LinqSTGNodeViewModel>
    {
        public static readonly Color DefaultTitleColor = Color.FromArgb(0xFF, 0x6B, 0xA0, 0xC7);

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(LinqSTGNodeViewModel), typeof(LinqSTGNodeView), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleColorProperty =
            DependencyProperty.Register(nameof(TitleColor), typeof(Color), typeof(LinqSTGNodeView), new PropertyMetadata(DefaultTitleColor));

        public LinqSTGNodeViewModel? ViewModel
        {
            get => (LinqSTGNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public Color TitleColor
        {
            get => (Color)GetValue(TitleColorProperty);
            set => SetValue(TitleColorProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (LinqSTGNodeViewModel?)value;
        }

        public LinqSTGNodeView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.NodeView.ViewModel).DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel)
                    .Select(v => (v?.TitleColor) ?? DefaultTitleColor)
                    .BindTo(this, v => v.TitleColor)
                    .DisposeWith(d);
            });
        }
    }
}
