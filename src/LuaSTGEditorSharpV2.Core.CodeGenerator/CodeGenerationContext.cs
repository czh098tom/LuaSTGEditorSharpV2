using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class CodeGenerationContext : NodeContext<CodeGenerationServiceSettings>
    {
        public static string ApplyMacroWithString(string original, string toBeReplaced, string @new)
        {
            Regex regex = new($"\\b{toBeReplaced}\\b"
                + @"(?<=^([^""]*((?<!(^|[^\\])(\\\\)*\\)""([^""]|((?<=(^|[^\\])(\\\\)*\\)""))*(?<!(^|[^\\])(\\\\)*\\)"")+)*[^""]*.)"
                + @"(?<=^([^']*((?<!(^|[^\\])(\\\\)*\\)'([^']|((?<=(^|[^\\])(\\\\)*\\)'))*(?<!(^|[^\\])(\\\\)*\\)')+)*[^']*.)");
            return regex.Replace(original, @new);
        }

        private readonly Dictionary<NodeData, Dictionary<string, string>> _contextVariables = new();

        public int IndentionLevel { get; private set; } = 0;
        public IReadOnlyDictionary<NodeData, Dictionary<string, string>> ContextVariables => _contextVariables;

        public CodeGenerationContext(LocalServiceParam localSettings, CodeGenerationServiceSettings serviceSettings) 
            : base(localSettings, serviceSettings) { }

        public void Push(NodeData current, int indentionIncrement)
        {
            IndentionLevel += indentionIncrement;
            _contextVariables.Add(current, new Dictionary<string, string>());
            Push(current);
        }

        public void Pop(int indentionIncrement)
        {
            IndentionLevel -= indentionIncrement;
            _contextVariables.Remove(Pop());
        }

        public StringBuilder GetIndented()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < IndentionLevel; i++)
            {
                sb.Append(ServiceSettings.IndentionString);
            }
            return sb;
        }

        public string ApplyMacro(string original)
        {
            foreach (var node in EnumerateTypeFromFarest("macro"))
            {
                if (node.HasProperty("replace") && node.HasProperty("by"))
                {
                    original = ApplyMacroWithString(original, node.GetProperty("replace"), node.GetProperty("by"));
                }
            }
            return original;
        }
    }
}
