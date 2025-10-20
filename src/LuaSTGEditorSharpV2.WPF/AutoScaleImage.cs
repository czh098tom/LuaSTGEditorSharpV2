using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LuaSTGEditorSharpV2.WPF
{
    public class AutoScaleImage : Image
    {
        protected override Size MeasureOverride(Size constraint)
        {
            if (Source is BitmapSource bmp)
            {
                double imgWidth = bmp.PixelWidth;
                double imgHeight = bmp.PixelHeight;

                double containerWidth = double.IsInfinity(constraint.Width) ? imgWidth : constraint.Width;

                if (imgWidth > containerWidth)
                {
                    double scale = containerWidth / imgWidth;
                    return new Size(containerWidth, imgHeight * scale);
                }
                else
                {
                    return new Size(imgWidth, imgHeight);
                }
            }
            return base.MeasureOverride(constraint);
        }
    }
}
