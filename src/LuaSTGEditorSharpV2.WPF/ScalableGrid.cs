using System;
using System.Collections.Generic;
using System.Linq;
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

namespace LuaSTGEditorSharpV2.WPF
{
    public partial class ScalableGrid : Grid, IScalable
    {
        public static readonly DependencyProperty ScaleProperty;

        static ScalableGrid()
        {
            ScaleProperty = ScalableHelper.RegisterScalePropertyFor(nameof(Scale),
                typeof(ScalableGrid));
        }

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        private readonly ScaleTransform _scaleTransform;

        public ScalableGrid() : base()
        {
            _scaleTransform = new ScaleTransform(Scale, Scale, 0, 0);
            LayoutTransform = _scaleTransform;

            CommandBindings.AddRange(new CommandBinding[]
            {
                new(ScalableHelper.ZoomIn, ZoomIn_Executed),
                new(ScalableHelper.ZoomOut, ZoomOut_Executed),
            });
        }

        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Scale += 0.1;
        }

        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Scale -= 0.1;
        }

        public void SetScale(double scale)
        {
            _scaleTransform.ScaleX = scale;
            _scaleTransform.ScaleY = scale;
        }
    }
}
