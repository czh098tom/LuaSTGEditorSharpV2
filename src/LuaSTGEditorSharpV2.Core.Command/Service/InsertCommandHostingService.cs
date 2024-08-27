using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Command.Factory;

namespace LuaSTGEditorSharpV2.Core.Command.Service
{
    [Inject(lifetime: ServiceLifetime.Singleton)]
    public class InsertCommandHostingService
    {
        public IInsertCommandFactory InsertCommandFactory { get; set; } = new InsertAfterFactory();
    }
}
