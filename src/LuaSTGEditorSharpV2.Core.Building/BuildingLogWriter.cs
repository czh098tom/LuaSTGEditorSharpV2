using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class BuildingLogWriter(Action<string> writer) : IBuildingLogWriter
    {
        public static IBuildingLogWriter Empty { get; } = new BuildingLogWriter(_ => { });

        public void WriteLine(string text)
        {
            writer.Invoke(text);
        }

        public static IBuildingLogWriter FromOutputLogWriter(IOutputLogWriter outputLogWriter)
        {
            return new FromOutputLog(outputLogWriter);
        }

        private class FromOutputLog(IOutputLogWriter outputLogWriter) : IBuildingLogWriter
        {
            private readonly IOutputLogWriter outputLogWriter = outputLogWriter;

            public void WriteLine(string text)
            {
                outputLogWriter.WriteLine("build", text);
            }
        }

        public static IBuildingLogWriter Create(Action<string> writer)
        {
            return new BuildingLogWriter(writer);
        }
    }
}
