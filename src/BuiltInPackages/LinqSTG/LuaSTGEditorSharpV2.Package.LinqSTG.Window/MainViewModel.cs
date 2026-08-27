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
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public class MainViewModel : INotifyPropertyChanged
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

        private ShootNode shootNode;

        public NodeCreationMenuViewModel NodeCreationMenu { get; } = new();

        /// <summary>
        /// The variable list docked at the left edge of the blueprint area. Its
        /// entries act as outer-scope variables for the preview evaluation.
        /// </summary>
        public VariableListViewModel VariableList { get; } = new();

        private IEnumerable<PointPrediction> pointPredictions = [];

        /// <summary>Latest pattern producers of all Shoot nodes; re-invoked whenever the variable list changes.</summary>
        private IReadOnlyList<Contextual<IEnumerable<PointPrediction>>> shootResults = Array.Empty<Contextual<IEnumerable<PointPrediction>>>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            shootNode = new ShootNode();
            network.Nodes.Add(shootNode);
            network.ConnectionFactory = (input, output) => new LinqSTGConnectionViewModel(network, input, output);

            var modifyNodes = network.Nodes
                .Connect()
                .ToCollection()
                .SelectMany(c => c
                    .OfType<ShootNode>()
                    .Select(s => s.Result)
                    .CombineLatest())
                .Subscribe(ls =>
                {
                    shootResults = ls.ToArray();
                    UpdatePattern();
                });

            VariableList.Changed += (_, _) => UpdatePattern();
        }

        /// <summary>
        /// Re-materializes the Shoot patterns against the current variable list:
        /// the list entries are seeded into the root parameter's float scope, so
        /// <see cref="ViewModel.Nodes.Data.PatternVariableNode"/> reads them by
        /// name during preview evaluation.
        /// </summary>
        private void UpdatePattern()
        {
            var root = new Parameter { Floats = new FloatScope(VariableList.ToFloats()) };
            pointPredictions = shootResults.SelectMany(pred => pred.Invoke(root));
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
        /// Creates a variable reference node for a variable list entry dropped
        /// into the blueprint area: int-typed entries produce the int variant,
        /// float-typed entries the float variant. The node starts bound to the
        /// entry's name.
        /// </summary>
        public void AddPatternVariableNode(string name, bool isInteger, System.Windows.Point position)
        {
            PatternVariableNodeBase node = isInteger
                ? new PatternVariableIntNode()
                : new PatternVariableFloatNode();
            node.Position = position;
            node.NameEditor.RawValue = name;
            Network.Nodes.Add(node);
        }

        public void Save()
        {
            try
            {
                NetworkJson = JsonConvert.SerializeObject(NetworkModelConversion.FromViewModel(network, VariableList));
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
                model?.ApplyToNetwork(network);
                VariableList.LoadFrom(model?.Variables);
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
    }
}
