using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public class SharpNodeFormattingContext
    {
        private readonly HashSet<NodeData> _parsedNodes = [];
        private readonly Dictionary<NodeData, int> _sourceLines = [];
        private readonly string _inputFilePath;
        private readonly ILogger _logger;

        public SharpNodeFormattingContext()
            : this(null, string.Empty, NullLogger.Instance)
        {
        }

        public SharpNodeFormattingContext(NodeData? root, string inputFilePath,
            ILogger logger)
        {
            _inputFilePath = inputFilePath;
            _logger = logger;
            var userDefinedNodes = UserDefinedNodeConverter.CollectTemplates(root);
            UserDefinedTemplates = userDefinedNodes.Templates;
            UserDefinedInstances = userDefinedNodes.Instances;
        }

        internal IReadOnlyDictionary<string, UserDefinedNodeConverter.UserDefinedTemplate>
            UserDefinedTemplates { get; }
        internal IReadOnlySet<NodeData> UserDefinedInstances { get; }

        public bool ShouldRetry { get; set; } = false;

        public void RegisterSourceLine(NodeData node, int line)
        {
            _sourceLines[node] = line;
        }

        public int? GetSourceLine(NodeData node)
            => _sourceLines.TryGetValue(node, out var line) ? line : null;

        public void ReportDiagnostic(NodeData node, string message)
        {
            _logger.LogWarning("{InputFilePath}:{SourceLine}: {Diagnostic}",
                _inputFilePath, GetSourceLine(node), message);
        }

        public void MakeParsed(NodeData nodeData)
        {
            _parsedNodes.Add(nodeData);
        }
    }
}
