using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core
{
    [Inject(ServiceLifetime.Transient)]
    public class PackedDataBase(IServiceProvider serviceProvider)
    {
        [JsonIgnore]
        protected IServiceProvider ServiceProvider { get; } = serviceProvider;
    }
}
