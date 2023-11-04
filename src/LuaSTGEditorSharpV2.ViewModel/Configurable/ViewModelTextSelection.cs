using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel.Configurable
{
    [Serializable]
    public record class ViewModelTextSelection(string Text, string ConditionOn, bool Inversed)
    {
        public bool ShouldAppend(NodeData source)
        {
            bool isTrue = source.GetProperty(ConditionOn, "true").ToLower().Trim() == "true";
            return isTrue && !Inversed || !isTrue && Inversed;
        }
    }
}
