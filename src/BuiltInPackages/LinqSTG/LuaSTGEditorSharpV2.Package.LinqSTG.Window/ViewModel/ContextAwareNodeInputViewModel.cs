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

                // 类型不匹配时上下文感知输入放宽为允许（类型随所连输出自适应），
                // 但成环拒绝必须保留：成环的值流会同步互相递归直至栈溢出。
                return new ConnectionValidationResult(
                    !ConnectionCycleDetector.WouldCreateCycle(this, pending.Output), null);
            };

            TypeChanged = ValueChanged.Select(v => v?.GetType());

            ValueChanged
                .Select(v => v?.GetType())
                .ToProperty(this, vm => vm.CurrentType,
                    out currentType, deferSubscription: false, Scheduler.Immediate);
        }
    }
}
