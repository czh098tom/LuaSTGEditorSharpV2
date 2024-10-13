using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Core
{
    /// <summary>
    /// Register a type as a service after framework is initialized.
    /// </summary>
    /// <param name="lifetime"> lifetime of service. </param>
    /// <param name="serviceType"> Declaring type of service. </param>
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
