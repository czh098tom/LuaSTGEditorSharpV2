using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InjectAttribute(Type? serviceType = null, ServiceLifetime lifetime = ServiceLifetime.Transient) 
        : Attribute
    {
        public ServiceDescriptor ToDescriptor(Type implementationType)
        {
            return new ServiceDescriptor(serviceType ?? implementationType, implementationType, lifetime);
        }
    }
}
