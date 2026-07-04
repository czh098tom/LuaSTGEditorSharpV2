using LinqSTG;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor
{
    public class IntervalTypeEditorViewModel : ValueEditorViewModel<Contextual<IntervalType>>, IContextualValueEditorViewModel<IntervalType>
    {
        private const IntervalType DefaultValue = IntervalType.HeadClosed;

        public IntervalType RawValue
        {
            get => Value?.Invoke(Parameter.Empty) ?? DefaultValue;
            set
            {
                HeadClosed = (value & IntervalType.HeadClosed) != 0;
                TailClosed = (value & IntervalType.TailClosed) != 0;
            }
        }

        private bool _headClosed = (DefaultValue & IntervalType.HeadClosed) != 0;
        public bool HeadClosed
        {
            get => _headClosed;
            set => this.RaiseAndSetIfChanged(ref _headClosed, value);
        }

        private bool _tailClosed = (DefaultValue & IntervalType.TailClosed) != 0;
        public bool TailClosed
        {
            get => _tailClosed;
            set => this.RaiseAndSetIfChanged(ref _tailClosed, value);
        }

        public IntervalTypeEditorViewModel()
        {
            Value = Contextual.Create(_ => DefaultValue);
            this.WhenAnyValue(x => x.HeadClosed, x => x.TailClosed)
                .Subscribe(flags =>
                {
                    var v = IntervalType.Open;
                    if (flags.Item1) v |= IntervalType.HeadClosed;
                    if (flags.Item2) v |= IntervalType.TailClosed;
                    Value = Contextual.Create(_ => v);
                });
        }
    }
}
