using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public static class OutputLogWriterExtension
    {
        public static IBuildingLogWriter ToBuildingLogWriter(this IOutputLogWriter outputLogWriter)
        {
            return BuildingLogWriter.FromOutputLogWriter(outputLogWriter);
        }
    }
}
