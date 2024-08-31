using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Core
{
    internal class ServiceProviderContractResolver(IServiceProvider serviceProvider) : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var objectContract = base.CreateObjectContract(objectType);

            if (objectType.GetCustomAttribute<InjectAttribute>() != null)
            {
                objectContract.DefaultCreator = () => serviceProvider.GetRequiredService(objectType);
            }

            return objectContract;
        }
    }
}
