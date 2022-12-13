using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public struct CodeData
    {
        public string Content { get; private set; }
        public NodeData Source { get; private set; }
        public int LineCount { get; private set; }

        public CodeData(string content, NodeData source)
        {
            Content = content;
            Source = source;
            LineCount = 0;
            for(int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n') LineCount++;
            }
        }
    }
}
