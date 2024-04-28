using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTasks
{
    public class NamedBuildtingTask(string name, IBuildingTask inner) : IBuildingTask
    {
        public string Name { get; private set; } = name;

        private readonly IBuildingTask _inner = inner;

        public async Task Execute(BuildingContext context,
            IProgress<float>? progressReporter = null,
            CancellationToken cancellationToken = default)
        {
            await _inner.Execute(context, progressReporter, cancellationToken);
        }
    }
}
