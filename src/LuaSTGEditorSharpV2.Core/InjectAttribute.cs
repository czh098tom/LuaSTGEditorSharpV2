using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute(ServiceLifetime lifetime, Type? serviceType = null) 
        : Attribute
    {
        public ServiceDescriptor ToDescriptor(Type implementationType)
        {
            return new ServiceDescriptor(serviceType ?? implementationType, implementationType, lifetime);
        }
    }
}
