using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class LanguageProviderService : PackedDataProviderServiceBase<LanguageBase>
    {
        public readonly LanguageBase _default = new();

        public LanguageBase? GetLanguage(string name)
        {
            return GetDataOfID(name);
        }
    }
}
