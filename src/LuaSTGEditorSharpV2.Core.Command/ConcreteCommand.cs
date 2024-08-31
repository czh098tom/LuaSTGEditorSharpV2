using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public abstract class ConcreteCommand(ViewModelProviderServiceProvider viewModelProviderServiceProvider) : CommandBase
    {
        protected ViewModelProviderServiceProvider ViewModelProviderServiceProvider { get; private set; } = viewModelProviderServiceProvider;
    }
}
