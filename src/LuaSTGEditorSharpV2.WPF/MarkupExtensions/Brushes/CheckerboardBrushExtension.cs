using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.WPF.MarkupExtensions.Brushes
{
    public enum CheckerboardCellSize
    {
        Small = 4,
        Medium = 8,
        Large = 12,
    }

    [MarkupExtensionReturnType(typeof(Brush))]
    public sealed class CheckerboardBrushExtension : MarkupExtension
    {
        private static readonly ConcurrentDictionary<CheckerboardCellSize, Brush> Cache = new();

        public CheckerboardBrushExtension()
        {
        }

        public CheckerboardBrushExtension(CheckerboardCellSize size)
        {
            Size = size;
        }

        public CheckerboardCellSize Size { get; set; } = CheckerboardCellSize.Medium;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Cache.GetOrAdd(Size, CreateBrush);
        }

        private static Brush CreateBrush(CheckerboardCellSize size)
        {
            int cell = (int)size;
            var brush = new DrawingBrush
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, cell * 2, cell * 2),
                ViewportUnits = BrushMappingMode.Absolute,
            };

            var drawing = new DrawingGroup();
            drawing.Children.Add(CreateCell(cell, 0, 0, System.Windows.Media.Brushes.White));
            drawing.Children.Add(CreateCell(cell, cell, 0, LightGrayBrush));
            drawing.Children.Add(CreateCell(cell, 0, cell, LightGrayBrush));
            drawing.Children.Add(CreateCell(cell, cell, cell, System.Windows.Media.Brushes.White));

            brush.Drawing = drawing;
            brush.Freeze();
            return brush;
        }

        private static GeometryDrawing CreateCell(double cell, double x, double y, Brush fill)
        {
            return new GeometryDrawing
            {
                Brush = fill,
                Geometry = new RectangleGeometry(new Rect(x, y, cell, cell)),
            };
        }

        private static readonly Brush LightGrayBrush =
            CreateFrozenSolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));

        private static SolidColorBrush CreateFrozenSolidColorBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
