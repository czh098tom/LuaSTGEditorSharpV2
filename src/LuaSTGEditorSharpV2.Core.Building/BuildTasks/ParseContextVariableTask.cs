using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTasks
{
    public class ParseContextVariableTask(IInputSourceVariable sourceVariable, 
        IOutputTargetVariable targetVariable,
        Func<string, string>? mapper = null) : IBuildingTask
    {
        private static readonly Func<string, string> _unit = s => s;

        public IInputSourceVariable SourceVariable { get; private set; } = sourceVariable;
        public IOutputTargetVariable TargetVariable { get; private set; } = targetVariable;

        private readonly Func<string, string> mapper = mapper ?? _unit;

        public async Task Execute(BuildingContext context, IProgress<float>? progressReporter = null, 
            CancellationToken cancellationToken = default)
        {
            TargetVariable.WriteTarget(SourceVariable.GetVariable(context)
                .Select(mapper)
                .ToArray(), context);
            await Task.CompletedTask;
        }
    }
}
