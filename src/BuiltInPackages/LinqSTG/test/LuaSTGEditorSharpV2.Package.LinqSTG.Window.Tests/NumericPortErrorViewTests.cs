using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.View;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using NodeNetwork.Views;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    public class NumericPortErrorViewTests
    {
        [Fact]
        public void PortError_IsRenderedAndClearsWithoutOpeningAWindow()
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var port = new LinqSTGPortViewModel(Colors.Blue);
                    var view = new LinqSTGPortView { ViewModel = port, Width = 40, Height = 40 };
                    view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                    var inner = (PortView)view.FindName("PortView");
                    inner.ApplyTemplate();
                    view.Measure(new Size(40, 40));
                    view.Arrange(new Rect(0, 0, 40, 40));
                    view.UpdateLayout();
                    view.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                    Assert.Equal(Colors.Blue, ((SolidColorBrush)inner.RegularStroke).Color);
                    port.EvaluationError = "Cannot convert NaN to Int32";
                    view.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                    Assert.Equal(Color.FromRgb(0xF4, 0x43, 0x36), ((SolidColorBrush)inner.RegularStroke).Color);
                    Assert.Equal(port.EvaluationError, view.ToolTip);
                    Render(view, "numeric-port-error.png");

                    // Connection validation changes must not erase an evaluation failure.
                    port.IsInErrorMode = true;
                    port.IsInErrorMode = false;
                    Assert.Equal(Color.FromRgb(0xF4, 0x43, 0x36), ((SolidColorBrush)inner.RegularStroke).Color);
                    port.EvaluationError = null;
                    view.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                    Assert.Equal(Colors.Blue, ((SolidColorBrush)inner.RegularStroke).Color);
                    Assert.Null(view.ToolTip);
                    Render(view, "numeric-port-recovered.png");
                    view.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
                finally
                {
                    Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
            }) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Port rendering test timed out.");
            Assert.Null(failure);
        }

        private static void Render(FrameworkElement view, string name)
        {
            var bitmap = new RenderTargetBitmap(40, 40, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(view);
            var directory = Environment.GetEnvironmentVariable("LINQSTG_PORT_RENDER_DIRECTORY");
            if (string.IsNullOrWhiteSpace(directory)) return;
            System.IO.Directory.CreateDirectory(directory);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = System.IO.File.Create(System.IO.Path.Combine(directory, name));
            encoder.Save(stream);
        }
    }
}
