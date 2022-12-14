using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class CodeGenerationContext : NodeContext
    {
        public int IndentionLevel { get; private set; } = 0;
        public Dictionary<NodeData, Dictionary<string, string>> ContextVariables = new();

        public CodeGenerationContext(LocalSettings localSettings) : base(localSettings) { }

        public void Push(NodeData current, int indentionIncrement)
        {
            IndentionLevel += indentionIncrement;
            ContextVariables.Add(current, new Dictionary<string, string>());
            Push(current);
        }

        public void Pop(int indentionIncrement)
        {
            IndentionLevel -= indentionIncrement;
            ContextVariables.Remove(Pop());
        }

        public StringBuilder GetIndented()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < IndentionLevel; i++)
            {
                sb.Append(LocalSettings.IndentionString);
            }
            return sb;
        }
    }
}
