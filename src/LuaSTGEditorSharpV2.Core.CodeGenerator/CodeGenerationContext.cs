using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class CodeGenerationContext : NodeContextWithSettings<CodeGenerationServiceSettings>
    {
        protected class CodeGenerationContextHandle : ContextHandle
        {
            private readonly CodeGenerationContext _codeGenerationContext;
            private readonly int _indentionIncrement;

            public CodeGenerationContextHandle(CodeGenerationContext context, int indentionIncrement)
                : base(context)
            {
                _codeGenerationContext = context;
                _indentionIncrement = indentionIncrement;
            }

            protected override void StepImpl(NodeData nodeData)
            {
                lock (_codeGenerationContext._lock)
                {
                    _codeGenerationContext.Push(nodeData, _indentionIncrement);
                }
            }

            protected override void DisposeImpl()
            {
                lock (_codeGenerationContext._lock)
                {
                    _codeGenerationContext.Pop(_indentionIncrement);
                }
            }
        }

        protected class CodeGenerationContextFormatter(CodeGenerationContext context) : ContextFormatter
        {
            protected override string GetToken(string? format, IFormatProvider formatProvider)
            {
                return format switch
                {
                    "IND" => context.ServiceSettings.IndentionString,
                    _ => string.Empty,
                };
            }
        }

        private CodeGeneratorServiceProvider CodeGeneratorServiceProvider =>
            ServiceProvider.GetRequiredService<CodeGeneratorServiceProvider>();

        private readonly Dictionary<NodeData, Dictionary<string, string>> _contextVariables = new();
        private readonly Stack<LanguageBase?> _languageEnvironment = new();
        private LanguageBase? _nearestLanguage = null;

        public int IndentionLevel { get; private set; } = 0;
        public IReadOnlyDictionary<NodeData, Dictionary<string, string>> ContextVariables => _contextVariables;

        public CodeGenerationContext(IServiceProvider serviceProvider, LocalServiceParam localSettings, CodeGenerationServiceSettings serviceSettings)
            : base(serviceProvider, localSettings, serviceSettings)
        {
            Formatter = new CodeGenerationContextFormatter(this);
        }

        public IDisposable AcquireContextHandle(NodeData current, int indentionIncrement = 0)
        {
            var handle = new CodeGenerationContextHandle(this, indentionIncrement);
            handle.Step(current);
            return handle;
        }

        public override IDisposable AcquireContextLevelHandle(NodeData current)
        {
            return AcquireContextHandle(current, 0);
        }

        protected void Push(NodeData current, int indentionIncrement)
        {
            IndentionLevel += indentionIncrement;
            _contextVariables.Add(current, []);
            var lang = CodeGeneratorServiceProvider.GetLanguageOfNode(current);
            _languageEnvironment.Push(lang);
            if (lang != null)
            {
                _nearestLanguage = lang;
            }
            PushBasicInfo(current);
        }

        protected NodeData Pop(int indentionIncrement)
        {
            var node = PopBasicInfo();
            IndentionLevel -= indentionIncrement;
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

        public StringBuilder ApplyIndentedFormat(string toAppend, params object?[] source)
        {
            return ApplyIndented(GetIndented(), Format(toAppend, source));
        }
    }
}
