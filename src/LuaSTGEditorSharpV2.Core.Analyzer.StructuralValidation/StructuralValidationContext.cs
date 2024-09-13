using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Analyzer.StructuralValidation
{
    public class StructuralValidationContext : NodeContextWithSettings<StructuralValidationServiceSettings>
    {
        public StructuralValidationContext(IServiceProvider serviceProvider, LocalServiceParam localSettings, StructuralValidationServiceSettings serviceSettings)
            : base(serviceProvider, localSettings, serviceSettings) { }
    }
}
