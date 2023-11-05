using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

namespace LuaSTGEditorSharpV2.Core
{
    public static class LocalizedResourceHost
    {
        private static readonly string _resourceName = "Resources.Text";

        private static readonly Dictionary<Assembly, ResourceManager> _resourceManagers = new();

        public static string? GetString(string key, Assembly assembly)
        {
            if (!_resourceManagers.ContainsKey(assembly))
            {
                _resourceManagers.Add(assembly, 
                    new ResourceManager($"{assembly.GetName().Name}.{_resourceName}", assembly));
            }
            return _resourceManagers[assembly].GetString(key);
        }
    }
}
