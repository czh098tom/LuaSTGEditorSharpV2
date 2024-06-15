using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public sealed class PackageDescriptor(IEnumerable<IDisposable> disposables, IEnumerable<Assembly> assemblies)
        : IDisposable
    {
        private IEnumerable<IDisposable> _packageServiceInstanceDisposeHandle = disposables;
        public IEnumerable<Assembly> Assemblies { get; private set; } = assemblies;

        public void Dispose()
        {
            foreach (var item in _packageServiceInstanceDisposeHandle)
            {
                item.Dispose();
            }
        }
    }
}
