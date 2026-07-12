using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public class BulletPreviewCanvas : FrameworkElement
    {
        public static readonly DependencyProperty BulletsProperty =
            DependencyProperty.Register(
                nameof(Bullets),
                typeof(IReadOnlyList<BulletVisual>),
                typeof(BulletPreviewCanvas),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly Pen OutlinePen = CreateFrozenPen(Brushes.Red, 1.0);

        private const double WorldOffset = 10000.0;

        public IReadOnlyList<BulletVisual>? Bullets
        {
            get => (IReadOnlyList<BulletVisual>?)GetValue(BulletsProperty);
            set => SetValue(BulletsProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            var bullets = Bullets;
            if (bullets == null || bullets.Count == 0) return;

            foreach (var b in bullets)
            {
                var x = b.Position.X + WorldOffset;
                var y = b.Position.Y + WorldOffset;
                var d = b.Diameter;

                if (b.Shape == BulletShape.Square)
                {
                    dc.DrawRectangle(null, OutlinePen, new Rect(x, y, d, d));
                }
                else
                {
                    var r = d / 2.0;
                    dc.DrawEllipse(null, OutlinePen, new Point(x + r, y + r), r, r);
                }
            }
        }

        private static Pen CreateFrozenPen(Brush brush, double thickness)
        {
            var pen = new Pen(brush, thickness);
            pen.Freeze();
            return pen;
        }
    }
}
