using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class LanguageProviderService : PackedDataProviderServiceBase<LanguageBase>
    {
        public readonly LanguageBase _default;

        public LanguageProviderService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _default = new(serviceProvider);
        }

        public LanguageBase? GetLanguage(string name)
        {
            return GetDataOfID(name);
        }
    }
}
