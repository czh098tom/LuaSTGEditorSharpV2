using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel.Configurable
{
    [Serializable]
    public record class ViewModelTextSelection(string Text, string? ConditionOn,
        bool UseExistanceInsteadOfBoolValue, bool Inversed)
    {
        public bool ShouldAppend(NodeData source)
        {
            if (ConditionOn == null)
            {
                return !Inversed;
            }
            bool isTrue;
            if (!UseExistanceInsteadOfBoolValue)
            {
                isTrue = source.GetProperty(ConditionOn, "true").ToLower().Trim() == "true";
            }
            else
            {
                isTrue = !string.IsNullOrWhiteSpace(source.GetProperty(ConditionOn, "true"));
            }
            return isTrue ^ Inversed;
        }
    }
}
