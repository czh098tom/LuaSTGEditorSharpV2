using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    public class LinqSTGPortViewModel(Color portColor) : PortViewModel()
    {
        public Color PortColor { get; protected init; } = portColor;

        private string? evaluationError;
        public string? EvaluationError
        {
            get => evaluationError;
            set
            {
                if (evaluationError == value) return;
                this.RaiseAndSetIfChanged(ref evaluationError, value);
                this.RaisePropertyChanged(nameof(DisplayColor));
            }
        }

        // Keep evaluation failures visible independently of pending-connection validation.
        public Color DisplayColor => EvaluationError is null ? PortColor : Color.FromRgb(0xF4, 0x43, 0x36);
    }
}
