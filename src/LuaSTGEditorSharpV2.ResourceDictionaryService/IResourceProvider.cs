using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ResourceDictionaryService
{
    public interface IResourceProvider
    {
        public IReadOnlyList<string> ResourceDictUris { get; }
    }
}
