using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Analyzer.StructuralValidation
{
    public class StructuralValidationContext : NodeContext<StructuralValidationServiceSettings>
    {
        public StructuralValidationContext(LocalSettings localSettings, StructuralValidationServiceSettings serviceSettings) 
            : base(localSettings, serviceSettings) { }
    }
}
