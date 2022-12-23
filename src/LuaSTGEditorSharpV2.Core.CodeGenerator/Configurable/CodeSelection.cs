using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable
{
    [Serializable]
    public record class CodeSelection(string Code, string ConditionOn, bool Inversed)
    {
        public bool ShouldAppend(NodeData source)
        {
            bool isTrue = source.Properties.GetValueOrDefault(ConditionOn, "true").ToLower().Trim() == "true";
            return (isTrue && !Inversed) || (!isTrue && Inversed);
        }
    }
}
