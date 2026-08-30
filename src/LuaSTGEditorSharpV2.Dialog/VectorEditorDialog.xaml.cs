using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LuaSTGEditorSharpV2.Dialog;

public partial class VectorEditorDialog : Window, INotifyPropertyChanged
{
    private bool? _clipToTen;
    private double _beginX;
    private double _beginY;
    private double _selectedX;
    private double _selectedY;
    private bool _headDragStarted;
    private bool _tailDragStarted;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double BeginX
    {
        get => _beginX;
        set
        {
            _selectedX += value - _beginX;
            _beginX = value;
            RaisePropertyChanged(nameof(BeginX));
            RaisePropertyChanged(nameof(SelectedX));
            RaisePropertyChanged(nameof(OffsetX));
            RaisePropertyChanged(nameof(OffsetY));
            RaisePropertyChanged(nameof(Radius));
            RaisePropertyChanged(nameof(Theta));
            RenderVector();
        }
    }

    public double BeginY
    {
        get => _beginY;
        set
        {
            _selectedY += value - _beginY;
            _beginY = value;
            RaisePropertyChanged(nameof(BeginY));
            RaisePropertyChanged(nameof(SelectedY));
            RaisePropertyChanged(nameof(OffsetX));
            RaisePropertyChanged(nameof(OffsetY));
            RaisePropertyChanged(nameof(Radius));
            RaisePropertyChanged(nameof(Theta));
            RenderVector();
        }
    }

    public double SelectedX
    {
        get => _selectedX;
        set
        {
            _selectedX = value;
            RaisePropertyChanged(nameof(SelectedX));
            RaisePropertyChanged(nameof(OffsetX));
            RaisePropertyChanged(nameof(Radius));
            RaisePropertyChanged(nameof(Theta));
            RenderVector();
        }
    }

    public double SelectedY
    {
        get => _selectedY;
        set
        {
            _selectedY = value;
            RaisePropertyChanged(nameof(SelectedY));
            RaisePropertyChanged(nameof(OffsetY));
            RaisePropertyChanged(nameof(Radius));
            RaisePropertyChanged(nameof(Theta));
            RenderVector();
        }
    }

    public double OffsetX
    {
        get => _selectedX - _beginX;
        set => SelectedX = _beginX + value;
    }

    public double OffsetY
    {
        get => _selectedY - _beginY;
        set => SelectedY = _beginY + value;
    }

    public double Radius
    {
        get => Math.Sqrt(OffsetX * OffsetX + OffsetY * OffsetY);
        set
        {
            var radius = Radius;
            if (radius < double.Epsilon)
            {
                OffsetX = value;
                OffsetY = 0;
                return;
            }

            var scale = value / radius;
            OffsetX *= scale;
            OffsetY *= scale;
            RaisePropertyChanged(nameof(Radius));
        }
    }

    public double Theta
    {
        get => Math.Atan2(OffsetY, OffsetX) / Math.PI * 180;
        set
        {
            var radius = Radius;
            var angle = value / 180 * Math.PI;
            OffsetX = Math.Cos(angle) * radius;
            OffsetY = Math.Sin(angle) * radius;
            RaisePropertyChanged(nameof(Theta));
        }
    }

    public VectorEditorDialog()
    {
        InitializeComponent();
        RenderVector();
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(DrawingCanvas);
        OffsetX = ScreenToX(point.X, _clipToTen) - BeginX;
        OffsetY = ScreenToY(point.Y, _clipToTen) - BeginY;
        _headDragStarted = true;
        DrawingCanvas.CaptureMouse();
    }

    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _headDragStarted = false;
        ReleaseMouseCaptureIfIdle();
    }

    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(DrawingCanvas);
        BeginX = ScreenToX(point.X, _clipToTen);
        BeginY = ScreenToY(point.Y, _clipToTen);
        _tailDragStarted = true;
        DrawingCanvas.CaptureMouse();
    }

    private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        _tailDragStarted = false;
        ReleaseMouseCaptureIfIdle();
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var point = e.GetPosition(DrawingCanvas);
        if (_headDragStarted)
        {
            OffsetX = ScreenToX(point.X, _clipToTen) - BeginX;
            OffsetY = ScreenToY(point.Y, _clipToTen) - BeginY;
        }
        else if (_tailDragStarted)
        {
            BeginX = ScreenToX(point.X, _clipToTen);
            BeginY = ScreenToY(point.Y, _clipToTen);
        }
    }

    private void ButtonOK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void NotClip_Click(object sender, RoutedEventArgs e) => _clipToTen = null;
    private void ClipTo1_Click(object sender, RoutedEventArgs e) => _clipToTen = false;
    private void ClipTo10_Click(object sender, RoutedEventArgs e) => _clipToTen = true;

    private void RenderVector()
    {
        if (!IsInitialized)
        {
            return;
        }

        var begin = new Point(LstgToScreenX(BeginX), LstgToScreenY(BeginY));
        var selected = new Point(LstgToScreenX(SelectedX), LstgToScreenY(SelectedY));
        vectorLine.X1 = begin.X;
        vectorLine.Y1 = begin.Y;
        vectorLine.X2 = selected.X;
        vectorLine.Y2 = selected.Y;
        vectorLineBorder.X1 = begin.X;
        vectorLineBorder.Y1 = begin.Y;
        vectorLineBorder.X2 = selected.X;
        vectorLineBorder.Y2 = selected.Y;

            System.Windows.Controls.Canvas.SetLeft(originMarker, begin.X - 5);
            System.Windows.Controls.Canvas.SetTop(originMarker, begin.Y - 5);
        SetLine(endCross1, selected.X - 5, selected.Y - 5, selected.X + 5, selected.Y + 5);
        SetLine(endCross2, selected.X - 5, selected.Y + 5, selected.X + 5, selected.Y - 5);
        SetLine(endCrossBorder1, selected.X - 5, selected.Y - 5, selected.X + 5, selected.Y + 5);
        SetLine(endCrossBorder2, selected.X - 5, selected.Y + 5, selected.X + 5, selected.Y - 5);
    }

    private static void SetLine(Line line, double x1, double y1, double x2, double y2)
    {
        line.X1 = x1;
        line.Y1 = y1;
        line.X2 = x2;
        line.Y2 = y2;
    }

    private void ReleaseMouseCaptureIfIdle()
    {
        if (!_headDragStarted && !_tailDragStarted)
        {
            DrawingCanvas.ReleaseMouseCapture();
        }
    }

    private static double ScreenToX(double x, bool? clipToTen)
    {
        var value = x - 224;
        return clipToTen switch
        {
            null => value,
            false => Convert.ToInt32(value),
            true => Convert.ToInt32(value / 10) * 10,
        };
    }

    private static double ScreenToY(double y, bool? clipToTen)
    {
        var value = 240 - y;
        return clipToTen switch
        {
            null => value,
            false => Convert.ToInt32(value),
            true => Convert.ToInt32(value / 10) * 10,
        };
    }

    private static double LstgToScreenX(double x) => x + 224;
    private static double LstgToScreenY(double y) => 240 - y;

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
