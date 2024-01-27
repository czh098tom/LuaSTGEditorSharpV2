using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Model
{
    public interface IDocument
    {
        public NodeData Root { get; }

        public string? FilePath { get; }

        public string FileName { get; }

        public bool IsOnDisk { get; }

        void Save();
        void SaveAs(string filePath);
    }
}
