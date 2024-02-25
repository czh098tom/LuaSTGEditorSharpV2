using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class CodeGenerationContext : NodeContextWithSettings<CodeGenerationServiceSettings>
    {
        private static CodeGeneratorServiceProvider CodeGeneratorServiceProvider =>
            HostedApplicationHelper.GetService<CodeGeneratorServiceProvider>();

        private readonly Dictionary<NodeData, Dictionary<string, string>> _contextVariables = new();
        private readonly Stack<LanguageBase?> _languageEnvironment = new();
        private LanguageBase? _nearestLanguage = null;

        public int IndentionLevel { get; private set; } = 0;
        public IReadOnlyDictionary<NodeData, Dictionary<string, string>> ContextVariables => _contextVariables;

        public CodeGenerationContext(LocalServiceParam localSettings, CodeGenerationServiceSettings serviceSettings)
            : base(localSettings, serviceSettings) { }

        public void Push(NodeData current, int indentionIncrement)
        {
            IndentionLevel += indentionIncrement;
            Push(current);
        }

        public NodeData Pop(int indentionIncrement)
        {
            IndentionLevel -= indentionIncrement;
            return Pop();
        }

        public override void Push(NodeData current)
        {
            _contextVariables.Add(current, []);
            var lang = CodeGeneratorServiceProvider.GetLanguageOfNode(current);
            _languageEnvironment.Push(lang);
            if (lang != null)
            {
                _nearestLanguage = lang;
            }
            base.Push(current);
        }

        public override NodeData Pop()
        {
            var node = base.Pop();
            _contextVariables.Remove(node);
            _languageEnvironment.Pop();
            if (_languageEnvironment.Peek() != null)
            {
                _nearestLanguage = _languageEnvironment.Peek();
            }
            return node;
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

        public string ApplyMacro(NodeData curr, string original)
        {
            foreach (var node in EnumerateTypeFromFarest("macro"))
            {
                if (node.HasProperty("replace") && node.HasProperty("by"))
                {
                    original = (CodeGeneratorServiceProvider.GetLanguageOfNode(curr) ?? _nearestLanguage)
                        ?.ApplyMacroWithString(original, node.GetProperty("replace"), node.GetProperty("by"))
                        ?? original;
                }
            }
            return original;
        }

        public StringBuilder ApplyIndented(StringBuilder indention, string toAppend)
        {
            var builder = new StringBuilder();
            string[] seg = toAppend.Split('\n');
            for (int i = 0; i < seg.Length; i++)
            {
                if (seg[i] != null)
                {
                    if (string.IsNullOrWhiteSpace(seg[i]))
                    {
                        if (!ServiceSettings.SkipBlankLine)
                        {
                            if (ServiceSettings.IndentOnBlankLine)
                            {
                                builder.Append(indention);
                                builder.Append(seg[i]);
                            }
                            if (!ServiceSettings.LineObfuscated)
                            {
                                builder.Append('\n');
                            }
                            else
                            {
                                builder.Append(' ');
                            }
                        }
                    }
                    else
                    {
                        builder.Append(indention);
                        builder.Append(seg[i]);
                        if (!ServiceSettings.LineObfuscated)
                        {
                            builder.Append('\n');
                        }
                        else
                        {
                            builder.Append(' ');
                        }
                    }
                }
            }
            return builder;
        }

        public StringBuilder ApplyIndentedFormat(StringBuilder indention
            , string toAppend, params object?[] source)
        {
            object?[] fullParams = new object[source.Length + 1];
            fullParams[0] = indention.ToString();
            Array.Copy(source, 0, fullParams, 1, source.Length);
            return ApplyIndented(indention, string.Format(toAppend, fullParams));
        }
    }
}
