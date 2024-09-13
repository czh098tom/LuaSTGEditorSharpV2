using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    [PackedServiceProvider]
    [ServiceShortName("sharpconv")]
    public class SharpNodeConverterServiceProvider(IServiceProvider serviceProvider) 
        : PackedDataProviderServiceBase<SharpNodeFormatConverter>(serviceProvider)
    {
        private readonly SharpNodeFormatConverter _default = new(serviceProvider);

        public SharpNodeFormatConverter GetConverter(string sharpNodeType)
        {
            return GetDataOfID(sharpNodeType) ?? _default;
        }
    }
}
