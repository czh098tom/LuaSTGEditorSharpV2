using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public interface IBuildingTask
    {
        public Task Execute(BuildingContext context, IProgress<float>? progressReporter = null,
            CancellationToken cancellationToken = default);
    }
}
