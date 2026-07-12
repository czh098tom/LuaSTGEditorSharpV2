using DynamicData;
using LinqSTG.Expression.ToLua.Serialization;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Serialization;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Transformation;
using Newtonsoft.Json;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator;

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

        public NodeListViewModel NodeList { get; } = new();

        private IEnumerable<PointPrediction> pointPredictions = [];

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
                    pointPredictions = ls.SelectMany(pred => pred.Invoke(Parameter.Empty));
                    UpdatePrediction();
                });

            NodeList.AddNodeType(() => new ConstantFloatNode());
            NodeList.AddNodeType(() => new ConstantIntNode());
            NodeList.AddNodeType(() => new ConstantStringNode());
            NodeList.AddNodeType(() => new RepeaterKeyNode());
            NodeList.AddNodeType(() => new Vector2FromRotationDistanceNode());
            NodeList.AddNodeType(() => new Vector2Node());

            NodeList.AddNodeType(() => new AddNode());
            NodeList.AddNodeType(() => new FloatToIntNode());
            NodeList.AddNodeType(() => new IntToFloatNode());
            NodeList.AddNodeType(() => new MinMaxNode());
            NodeList.AddNodeType(() => new Sample01Node());
            NodeList.AddNodeType(() => new Sample01MinMaxNode());
            NodeList.AddNodeType(() => new TakeRepeaterFromContextNode());
            NodeList.AddNodeType(() => new TakeVariableFromContextNode());

            NodeList.AddNodeType(() => new StationaryMovementNode());
            NodeList.AddNodeType(() => new UniformVelocityMovementNode());
            NodeList.AddNodeType(() => new UniformAccelerationMovementNode());

            NodeList.AddNodeType(() => new MovementSumNode());
            NodeList.AddNodeType(() => new MovementOffsetNode());
            NodeList.AddNodeType(() => new MovementAfterTimeNode());

            NodeList.AddNodeType(() => new RepeatPatternNode());
            NodeList.AddNodeType(() => new RepeatWithIntervalPatternNode());
            NodeList.AddNodeType(() => new SingleDataPatternNode());
            NodeList.AddNodeType(() => new SingleIntervalPatternNode());
            NodeList.AddNodeType(() => new EmptyPatternNode());

            NodeList.AddNodeType(() => new MapPatternNode());
            NodeList.AddNodeType(() => new ExtrudePatternNode());
            NodeList.AddNodeType(() => new ExtrudeConcatPatternNode());
            NodeList.AddNodeType(() => new FilterPatternNode());
            NodeList.AddNodeType(() => new ConcatPatternNode());
            NodeList.AddNodeType(() => new ReversePatternNode());
            NodeList.AddNodeType(() => new SkipPatternNode());
            NodeList.AddNodeType(() => new TakePatternNode());
            NodeList.AddNodeType(() => new SkipWhilePatternNode());
            NodeList.AddNodeType(() => new TakeWhilePatternNode());
            NodeList.AddNodeType(() => new TrimStartPatternNode());
            NodeList.AddNodeType(() => new TrimEndPatternNode());
            NodeList.AddNodeType(() => new TrimPatternNode());

            NodeList.AddNodeType(() => new AssignNode());

            NodeList.AddNodeType(() => new ShootNode());
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

        public void Save()
        {
            try
            {
                NetworkJson = JsonConvert.SerializeObject(NetworkModelConversion.FromViewModel(network));
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
