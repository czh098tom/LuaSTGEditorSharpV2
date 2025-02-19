using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public interface IPackedServiceInstanceCollection
    {
        IReadOnlyDictionary<string, (object data, PackageInfo packageInfo)> GetRegisteredAvailableData();
        IReadOnlyDictionary<string, IEnumerable<(object data, PackageInfo packageInfo)>> GetAllRegistered();
    }
}
