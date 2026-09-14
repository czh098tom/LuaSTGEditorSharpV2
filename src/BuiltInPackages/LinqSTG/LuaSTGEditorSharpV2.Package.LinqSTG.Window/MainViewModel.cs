using DynamicData;
using LinqSTG.Expression.ToLua.Serialization;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Serialization;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.NodeCreationMenu;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using Newtonsoft.Json;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public string? NetworkJson { get; set; }

        public IReadOnlyList<BulletVisual> Points { get; private set; } = Array.Empty<BulletVisual>();

        public int Time
        {
            get => time;
            set
            {
                time = value;
                if (isPlaying)
                {
                    timeAtPlayStart = value;
                    playStopwatch.Restart();
                }
                RaisePropertyChanged();
                UpdatePrediction();
            }
        }
        private int time;

        public int TimeMinimum
        {
            get => timeMinimum;
            set
            {
                timeMinimum = value;
                if (Time < timeMinimum)
                {
                    Time = timeMinimum;
                }
                RaisePropertyChanged();
            }
        }
        private int timeMinimum = 0;

        public int TimeMaximum
        {
            get => timeMaximum;
            set
            {
                timeMaximum = value;
                if (Time > timeMaximum)
                {
                    Time = timeMaximum;
                }
                RaisePropertyChanged();
            }
        }
        private int timeMaximum = 1200;

        private const double PlayUnitsPerSecond = 60.0;

        public bool IsPlaying
        {
            get => isPlaying;
            private set
            {
                if (isPlaying == value) return;
                isPlaying = value;
                RaisePropertyChanged();
            }
        }
        private bool isPlaying;

        private DispatcherTimer? playTimer;
        private readonly Stopwatch playStopwatch = new();
        private int timeAtPlayStart;

        public void Play()
        {
            if (isPlaying) return;
            if (Time >= TimeMaximum)
            {
                Time = TimeMinimum;
            }
            timeAtPlayStart = Time;
            playStopwatch.Restart();
            playTimer ??= CreatePlayTimer();
            playTimer.Start();
            IsPlaying = true;
        }

        public void Pause()
        {
            if (!isPlaying) return;
            playTimer!.Stop();
            playStopwatch.Stop();
            IsPlaying = false;
        }

        private DispatcherTimer CreatePlayTimer()
        {
            var timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(1000.0 / PlayUnitsPerSecond)
            };
            timer.Tick += OnPlayTick;
            return timer;
        }

        private void OnPlayTick(object? sender, EventArgs e)
        {
            var newTime = timeAtPlayStart + (int)Math.Round(playStopwatch.Elapsed.TotalSeconds * PlayUnitsPerSecond);
            if (newTime >= TimeMaximum)
            {
                time = TimeMaximum;
                RaisePropertyChanged(nameof(Time));
                UpdatePrediction();
                Pause();
                return;
            }
            time = newTime;
            RaisePropertyChanged(nameof(Time));
            UpdatePrediction();
        }

        public NetworkViewModel Network
        {
            get => network;
            set
            {
                network = value;
                RaisePropertyChanged();
            }
        }
        private NetworkViewModel network = new();

        private readonly CompositeDisposable disposables = new();
        private readonly SerialDisposable previewResultSubscription = new();
        private LinqSTGNodeViewModel? activePreviewSource;
        private Contextual<IEnumerable<PointPrediction>>? activePreviewResult;
        private bool isUpdatingPreviewSelection;

        public NodeCreationMenuViewModel NodeCreationMenu { get; } = new();

        /// <summary>
        /// The variable list docked at the left edge of the blueprint area. Its
        /// entries act as outer-scope variables for the preview evaluation.
        /// </summary>
        public VariableListViewModel VariableList { get; } = new();

        /// <summary>
        /// Random seed for the preview evaluation, configured in the bar docked
        /// to the bottom of the blueprint area. It is seeded into the root
        /// parameter so every random node derives its values from it, making
        /// the preview reproducible and stable while dragging the progress
        /// slider.
        /// </summary>
        public int Seed
        {
            get => seed;
            set
            {
                seed = value;
                RaisePropertyChanged();
                UpdatePreviewPattern();
            }
        }
        private int seed;

        private IEnumerable<PointPrediction> pointPredictions = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            network.ConnectionFactory = (input, output) => new LinqSTGConnectionViewModel(network, input, output);

            previewResultSubscription.DisposeWith(disposables);
            network.Nodes
                .Connect()
                .SubscribeMany(ObservePreviewSource)
                .Subscribe()
                .DisposeWith(disposables);

            var shootNode = new ShootNode { IsPreviewEnabled = true };
            network.Nodes.Add(shootNode);

            VariableList.Changed += (_, _) => UpdatePreviewPattern();
        }

        private Parameter CreatePreviewParameter()
        {
            return new Parameter
            {
                Floats = new FloatScope(VariableList.ToFloats()),
                Vectors = new VectorScope(VariableList.ToVectors()),
                Randomizer = new Random(seed),
            };
        }

        private void UpdatePreviewPattern()
        {
            pointPredictions = activePreviewResult?.Invoke(CreatePreviewParameter()).ToArray() ?? [];
            UpdatePrediction();
        }

        private void UpdatePrediction()
        {
            var list = new List<BulletVisual>();
            foreach (var pred in pointPredictions)
            {
                if (Time >= pred.StartTime)
                {
                    var point = pred.PointFunc.Predict(Time - pred.StartTime);
                    if (float.IsNaN(point.X) || float.IsNaN(point.Y)) continue;
                    var half = pred.Diameter / 2f;
                    list.Add(new BulletVisual(
                        new PointF(point.X - half, -point.Y - half),
                        pred.Shape,
                        pred.Diameter));
                }
            }
            Points = list;
            RaisePropertyChanged(nameof(Points));
        }

        private IDisposable ObservePreviewSource(NodeViewModel node)
        {
            if (node is not LinqSTGNodeViewModel source || !source.SupportsPreview)
            {
                return Disposable.Empty;
            }

            PropertyChangedEventHandler handler = (_, e) =>
            {
                if (e.PropertyName == nameof(LinqSTGNodeViewModel.IsPreviewEnabled))
                {
                    OnPreviewEnabledChanged(source);
                }
            };
            source.PropertyChanged += handler;

            if (source.IsPreviewEnabled)
            {
                ActivatePreviewSource(source);
            }

            return Disposable.Create(() =>
            {
                source.PropertyChanged -= handler;
                if (ReferenceEquals(activePreviewSource, source))
                {
                    ClearPreviewSource();
                }
            });
        }

        private void OnPreviewEnabledChanged(LinqSTGNodeViewModel source)
        {
            if (isUpdatingPreviewSelection)
            {
                return;
            }
            if (source.IsPreviewEnabled)
            {
                ActivatePreviewSource(source);
            }
            else if (ReferenceEquals(activePreviewSource, source))
            {
                ClearPreviewSource();
            }
        }

        private void ActivatePreviewSource(LinqSTGNodeViewModel source)
        {
            isUpdatingPreviewSelection = true;
            try
            {
                foreach (var other in network.Nodes.Items
                    .OfType<LinqSTGNodeViewModel>()
                    .Where(node => !ReferenceEquals(node, source) && node.SupportsPreview && node.IsPreviewEnabled))
                {
                    other.IsPreviewEnabled = false;
                }
            }
            finally
            {
                isUpdatingPreviewSelection = false;
            }

            activePreviewSource = source;
            activePreviewResult = null;
            pointPredictions = [];
            UpdatePrediction();
            previewResultSubscription.Disposable = source.PreviewResult!.Subscribe(
                result =>
                {
                    activePreviewResult = result;
                    UpdatePreviewPattern();
                },
                _ => ClearPreviewSource());
        }

        private void ClearPreviewSource()
        {
            activePreviewSource = null;
            activePreviewResult = null;
            previewResultSubscription.Disposable = Disposable.Empty;
            pointPredictions = [];
            UpdatePrediction();
        }

        /// <summary>
        /// Creates a new node of the given catalog entry at the specified network position,
        /// as picked from the blueprint area right-click creation menu.
        /// </summary>
        public void AddNode(NodeCreationEntry entry, System.Windows.Point position)
        {
            var node = entry.CreateNode();
            node.Position = position;
            Network.Nodes.Add(node);
        }

        /// <summary>
        /// Creates the node for a variable list entry dropped into the blueprint
        /// area. Locked built-ins generate their dedicated node types (自身坐标 /
        /// 玩家坐标); unlocked entries generate the generic variable reference
        /// nodes, int-typed entries the int variant, float-typed entries the float
        /// variant. (无限 is not a list entry: it is inserted from the right-click
        /// menu as an <see cref="InfiniteNode"/>.)
        /// </summary>
        public void AddNodeForVariable(VariableItemViewModel item, System.Windows.Point position)
        {
            LinqSTGNodeViewModel node;
            if (item.IsLocked)
            {
                node = item.Name switch
                {
                    VariableListViewModel.SelfName => new SelfPositionNode(),
                    VariableListViewModel.PlayerName => new PlayerPositionNode(),
                    // Unknown locked entries degrade to the generic references.
                    _ => item.IsInteger ? new PatternVariableIntNode() : new PatternVariableFloatNode(),
                };
            }
            else
            {
                node = item.IsInteger ? new PatternVariableIntNode() : new PatternVariableFloatNode();
            }
            node.Position = position;
            if (node is PatternVariableNodeBase variable)
            {
                variable.NameEditor.RawValue = item.Name;
            }
            Network.Nodes.Add(node);
        }

        public void Save()
        {
            try
            {
                var model = NetworkModelConversion.FromViewModel(network, VariableList) with { Seed = seed };
                NetworkJson = JsonConvert.SerializeObject(model);
            }
            catch (Exception)
            {
                NetworkJson = null;
            }
        }

        public void Load()
        {
            if (string.IsNullOrEmpty(NetworkJson))
            {
                return;
            }
            try
            {
                var model = JsonConvert.DeserializeObject<NetworkModel>(NetworkJson);
                if (model is null)
                {
                    return;
                }
                Seed = model.Seed;
                bool hasStoredPreviewSelection = model.Nodes.Any(node => node.PreviewEnabled.HasValue);
                model.ApplyToNetwork(network);
                VariableList.LoadFrom(model.Variables);
                if (!hasStoredPreviewSelection)
                {
                    var defaultPreviewSource = network.Nodes.Items.OfType<ShootNode>().FirstOrDefault();
                    if (defaultPreviewSource is not null)
                    {
                        defaultPreviewSource.IsPreviewEnabled = true;
                    }
                }
            }
            catch (Exception)
            {
                network.Connections.Clear();
                network.Nodes.Clear();
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }

        public void Dispose()
        {
            Pause();
            disposables.Dispose();
        }
    }
}
