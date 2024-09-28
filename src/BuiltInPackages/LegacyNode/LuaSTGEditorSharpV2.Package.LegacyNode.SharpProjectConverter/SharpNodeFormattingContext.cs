using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public class SharpNodeFormattingContext
    {
        private readonly HashSet<NodeData> _parsedNodes = [];

        public bool ShouldRetry { get; set; } = false;

        public void MakeParsed(NodeData nodeData)
        {
            _parsedNodes.Add(nodeData);
        }
    }
}
