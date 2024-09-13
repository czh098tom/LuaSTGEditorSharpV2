using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.ViewModel
{
    [Inject(ServiceLifetime.Transient)]
    public abstract class InjectableViewModel(IServiceProvider serviceProvider) : ViewModelBase
    {
        protected IServiceProvider ServiceProvider { get; private set; } = serviceProvider;
    }
}
