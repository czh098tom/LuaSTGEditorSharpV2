using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class DocumentPathSource : IInputSourceVariable
    {
        public IReadOnlyList<string> GetVariable(BuildingContext context)
        {
            return [context.LocalParam.Source.FilePath ?? throw new InvalidOperationException()];
        }
    }
}
