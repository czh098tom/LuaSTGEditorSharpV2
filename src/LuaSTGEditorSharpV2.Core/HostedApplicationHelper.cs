using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LuaSTGEditorSharpV2.Core
{
    public static class HostedApplicationHelper
    {
        public static IEnumerable<T> GetServicesWithInheritance<T>(this IServiceProvider services) where T : class
        {
            var collection = services.GetRequiredService<IServiceCollection>();
            return collection
                .Select(desc => desc.ServiceType)
                .Where(t => t.IsAnyDerivedTypeOf(typeof(T)))
                .Select(services.GetRequiredService)
                .OfType<T>() ?? throw new InvalidOperationException();
        }
    }
}
