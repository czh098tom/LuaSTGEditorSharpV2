using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public record struct WeightedBuildingTask(IBuildingTask BuildingTask, float Weight = 1f) { }
}
