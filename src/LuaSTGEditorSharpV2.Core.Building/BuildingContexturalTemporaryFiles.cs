using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public sealed class BuildingContexturalTemporaryFiles : IDisposable
    {
        private readonly string temporaryFolderPath = Path.GetTempPath();

        private readonly List<string> tempFiles = [];

        public BuildingContexturalTemporaryFiles() { }
        public BuildingContexturalTemporaryFiles(string temporaryFolderPath)
        {
            this.temporaryFolderPath = temporaryFolderPath;
        }

        public string AcquireTempFile()
        {
            var tempFileName = Path.Combine(temporaryFolderPath, GetTemporyName());
            tempFiles.Add(tempFileName);
            return tempFileName;
        }

#pragma warning disable CA1822
        private string GetTemporyName()
#pragma warning restore CA1822
        {
            return Guid.NewGuid().ToString();
        }

        public void Dispose()
        {
            foreach (var file in tempFiles)
            {
                File.Delete(file);
            }
        }
    }
}
