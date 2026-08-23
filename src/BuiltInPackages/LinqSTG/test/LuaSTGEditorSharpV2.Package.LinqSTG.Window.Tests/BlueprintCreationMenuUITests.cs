using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu;
using Nodes = LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Xunit;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Window.Tests
{
    /// <summary>
    /// End-to-end smoke test of the blueprint area right-click creation menu on a real
    /// window: opening the popup, culture-aware search, and creating a node at the
    /// right-clicked position.
    /// </summary>
    public class BlueprintCreationMenuUITests
    {
        private sealed class UiResult
        {
            public Exception? Error { get; set; }
            public bool RightClickHandled { get; set; }
            public bool PopupOpened { get; set; }
            public int CategoryCount { get; set; }
            public string CategoryNames { get; set; } = string.Empty;
            public int EnglishSearchCount { get; set; }
            public string EnglishSearchFirst { get; set; } = string.Empty;
            public int ChineseSearchCount { get; set; }
            public string ChineseSearchFirst { get; set; } = string.Empty;
            public string ChineseCategoryNames { get; set; } = string.Empty;
            public bool NodeCreated { get; set; }
            public int NodeCountAfter { get; set; }
            public Point CreatedPosition { get; set; }
            public bool PopupClosedAfterSelection { get; set; }
            public bool ReopenedPopup { get; set; }
            public bool CutActiveSuppressesPopup { get; set; }
            public string CutDiagnostics { get; set; } = "";
            public bool PopupOpensAfterCutFinished { get; set; }
        }

        private static UiResult RunOnSta()
        {
            var result = new UiResult();
            var thread = new Thread(() =>
            {
                // STA threads do not inherit the creating thread's culture; pin it so the
                // English assertions below are deterministic regardless of the OS language.
                Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
                var window = new BlueprintPatternWindow { Width = 1200, Height = 600 };
                try
                {
                    window.Show();
                    window.UpdateLayout();
                    FlushDispatcher();

                    var networkView = GetField<NetworkView>(window, "NetworkView");
                    var popup = GetField<Popup>(window, "NodeCreationPopup");
                    var menuView = GetField<FrameworkElement>(window, "NodeCreationMenu");
                    var mainViewModel = (MainViewModel)window.DataContext;
                    var network = mainViewModel.Network;
                    var menu = (NodeCreationMenuViewModel)menuView.DataContext;
                    Assert.NotNull(menu);

                    var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                    {
                        RoutedEvent = UIElement.PreviewMouseRightButtonUpEvent,
                        Source = networkView,
                    };
                    networkView.RaiseEvent(args);
                    result.RightClickHandled = args.Handled;
                    // the window defers opening to after the input event; flush the dispatcher queue
                    FlushDispatcher();
                    result.PopupOpened = popup.IsOpen;

                    result.CategoryCount = menu.Categories.Count;
                    result.CategoryNames = string.Join(",", menu.Categories.Select(c => c.Name));

                    menu.SearchText = "sin";
                    result.EnglishSearchCount = menu.SearchResults.Count;
                    result.EnglishSearchFirst = menu.SearchResults.FirstOrDefault()?.Entry.NodeType.Name ?? "";

                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
                    menu.Reload();
                    result.ChineseCategoryNames = string.Join(",", menu.Categories.Select(c => c.Name));
                    menu.SearchText = "浮点";
                    result.ChineseSearchCount = menu.SearchResults.Count;
                    result.ChineseSearchFirst = menu.SearchResults.FirstOrDefault()?.Entry.NodeType.Name ?? "";

                    menu.SearchText = string.Empty;
                    int before = network.Nodes.Count;
                    var entry = menu.AllEntries.Single(e => e.NodeType.Name == "SinNode");
                    var expectedPosition = new Point(123.5, -7.25);
                    typeof(BlueprintPatternWindow)
                        .GetField("_pendingNodePosition", BindingFlags.NonPublic | BindingFlags.Instance)!
                        .SetValue(window, expectedPosition);
                    typeof(BlueprintPatternWindow)
                        .GetMethod("NodeCreationMenu_NodeSelected", BindingFlags.NonPublic | BindingFlags.Instance)!
                        .Invoke(window, new object[]
                        {
                            new NodeCreationNodeItemViewModel
                            {
                                Entry = entry, Title = entry.Title, EnglishTitle = entry.EnglishTitle, Subtitle = "",
                            },
                        });
                    var created = network.Nodes.Items
                        .OfType<Nodes.LinqSTGNodeViewModel>()
                        .FirstOrDefault(n => n.GetType().Name == "SinNode");
                    result.NodeCreated = created != null;
                    result.NodeCountAfter = network.Nodes.Count;
                    result.CreatedPosition = created?.Position ?? new Point(double.NaN, double.NaN);
                    result.PopupClosedAfterSelection = !popup.IsOpen;

                    networkView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                    {
                        RoutedEvent = UIElement.PreviewMouseRightButtonUpEvent,
                        Source = networkView,
                    });
                    FlushDispatcher();
                    result.ReopenedPopup = popup.IsOpen;

                    // while the cut-connections line is active, releasing the right button
                    // must finish the cut instead of opening the menu
                    popup.IsOpen = false;
                    network.CutLine.StartPoint = new Point(10, 10);
                    network.CutLine.EndPoint = new Point(20, 20);
                    network.StartCut();
                    result.CutDiagnostics = $"cutVisible={network.CutLine.IsVisible}";
                    networkView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                    {
                        RoutedEvent = UIElement.PreviewMouseRightButtonUpEvent,
                        Source = networkView,
                    });
                    FlushDispatcher();
                    result.CutActiveSuppressesPopup = !popup.IsOpen;
                    network.FinishCut();

                    networkView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                    {
                        RoutedEvent = UIElement.PreviewMouseRightButtonUpEvent,
                        Source = networkView,
                    });
                    FlushDispatcher();
                    result.PopupOpensAfterCutFinished = popup.IsOpen;
                }
                catch (Exception ex)
                {
                    result.Error = ex;
                }
                finally
                {
                    window.Close();
                    Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }

        /// <summary>
        /// Pumps the STA thread's dispatcher until every queued operation above background
        /// priority has run (the window defers opening the popup through BeginInvoke).
        /// </summary>
        private static void FlushDispatcher()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }

        private static T GetField<T>(object obj, string name) where T : class
        {
            return (T)(typeof(BlueprintPatternWindow)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj)
                ?? throw new InvalidOperationException($"Field '{name}' not found."));
        }

        [Fact]
        public void RightClickOpensMenuSearchesAndCreatesNodes()
        {
            var result = RunOnSta();

            Assert.Null(result.Error);
            Assert.True(result.RightClickHandled, "right-click should be handled by the window");
            Assert.True(result.PopupOpened, "the creation popup should open on right-click");
            Assert.True(result.CategoryCount >= 8, $"expected the main categories, got: {result.CategoryNames}");
            Assert.Contains("Operator", result.CategoryNames.Split(','));

            Assert.True(result.EnglishSearchCount > 0, "English search 'sin' should find nodes");
            Assert.Equal("SinNode", result.EnglishSearchFirst);

            Assert.True(result.ChineseSearchCount > 0, "Chinese search '浮点' should find the float node");
            Assert.Equal("ConstantFloatNode", result.ChineseSearchFirst);
            Assert.Contains("运算符", result.ChineseCategoryNames);

            Assert.True(result.NodeCreated, "selecting a result should create the node");
            Assert.Equal(new Point(123.5, -7.25), result.CreatedPosition);
            Assert.True(result.PopupClosedAfterSelection, "popup should close after selecting a node");
            Assert.True(result.ReopenedPopup, "right-clicking again should reopen the popup");
            Assert.True(result.CutActiveSuppressesPopup,
                $"an active cut line must suppress the creation popup ({result.CutDiagnostics})");
            Assert.True(result.PopupOpensAfterCutFinished, "after the cut finished, right-click should open the popup again");
        }
    }
}
