using LuaSTGEditorSharpV2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [PackedServiceProvider]
    [ServiceShortName("cboptions")]
    public class ComboboxOptionsServiceProvider(IServiceProvider serviceProvider) 
        : PackedDataProviderServiceBase<ComboboxOptions>(serviceProvider)
    {
        private readonly ComboboxOptions _defaultOptions = new(serviceProvider);

        public ComboboxOptions GetComboboxOptions(string key)
        {
            return GetDataOfID(key) ?? _defaultOptions;
        }
    }
}
