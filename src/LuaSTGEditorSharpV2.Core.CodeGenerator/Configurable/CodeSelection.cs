using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable
{
    [Serializable]
    public record class CodeSelection(string Code, string ConditionOn, bool Inversed
        , bool NullDefault = true, bool UseExistance = false)
    {
        public bool ShouldAppend(NodeData source)
        {
            if (!UseExistance)
            {
                bool isTrue = source.GetProperty(ConditionOn, NullDefault.ToString()).ToLower().Trim() == "true";
                return (isTrue && !Inversed) || (!isTrue && Inversed);
            }
            else
            {
                bool isTrue = !string.IsNullOrWhiteSpace(source.GetProperty(ConditionOn, ""));
                return (isTrue && !Inversed) || (!isTrue && Inversed);
            }
        }
    }
}
