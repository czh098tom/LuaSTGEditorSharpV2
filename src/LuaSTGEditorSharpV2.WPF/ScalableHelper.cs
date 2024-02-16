using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LuaSTGEditorSharpV2.WPF
{
    public static class ScalableHelper
    {
        public static readonly RoutedUICommand ZoomIn;
        public static readonly RoutedUICommand ZoomOut;

        static ScalableHelper()
        {
            ZoomIn = new RoutedUICommand("Zoom In", "ZoomIn", typeof(ScalableHelper),
                [new KeyGesture(Key.OemPlus, ModifierKeys.Control)]);
            ZoomOut = new RoutedUICommand("Zoom Out", "ZoomOut", typeof(ScalableHelper),
                [new KeyGesture(Key.OemMinus, ModifierKeys.Control)]);
        }

        public static DependencyProperty RegisterScalePropertyFor(string name, Type t)
        {
            return DependencyProperty.Register(
                name,
                typeof(double),
                t,
                new PropertyMetadata(1.0, OnScalePropertyChanged, CoerceScaleProperty));
        }

        private static void OnScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not IScalable sg) return;
            var scaleVal = (double)e.NewValue;
            sg.SetScale(scaleVal);
        }

        private static object CoerceScaleProperty(DependencyObject d, object value)
        {
            var val = (double)value;
            if (double.IsNaN(val)) return 1.0;
            val = Math.Clamp(val, 0.1, 100);
            return val;
        }
    }
}
