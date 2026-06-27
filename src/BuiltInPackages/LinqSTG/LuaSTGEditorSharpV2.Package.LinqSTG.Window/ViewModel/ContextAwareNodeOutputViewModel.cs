using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    public class ContextAwareNodeOutputViewModel : LinqSTGNodeOutputViewModel<object?>
    {
        private IObservable<Type?>? type;

        public IObservable<Type?>? Type
        {
            get
            {
                return type;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref type, value, nameof(Type));
            }
        }

        public Type? CurrentType => currentType.Value;

        private readonly ObservableAsPropertyHelper<Type?> currentType;

        public ContextAwareNodeOutputViewModel()
        {
            this.WhenAnyObservable(vm => vm.Type)
                .ToProperty(this, vm => vm.CurrentType,
                    out currentType, deferSubscription: false, Scheduler.Immediate);
        }
    }
}
