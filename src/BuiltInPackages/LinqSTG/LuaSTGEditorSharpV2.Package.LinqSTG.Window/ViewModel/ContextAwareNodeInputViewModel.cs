using LinqSTG;
using NodeNetwork;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    public class ContextAwareNodeInputViewModel : LinqSTGNodeInputViewModel<object>
    {
        public ICollection<Type> Types { get; } = [];

        public IObservable<Type?> TypeChanged { get; }

        public Type? CurrentType => currentType.Value;

        private readonly ObservableAsPropertyHelper<Type?> currentType;

        public ContextAwareNodeInputViewModel(ICollection<Type> types)
            : base()
        {
            var originalValidator = ConnectionValidator;

            Types = types;

            ConnectionValidator = pending =>
            {
                var result = originalValidator(pending);
                if (result.IsValid) return result;

                return new ConnectionValidationResult(true, null);
            };

            TypeChanged = ValueChanged.Select(v => v?.GetType());

            ValueChanged
                .Select(v => v?.GetType())
                .ToProperty(this, vm => vm.CurrentType,
                    out currentType, deferSubscription: false, Scheduler.Immediate);
        }
    }
}
