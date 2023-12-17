using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable
{
    [Serializable]
    public record class CodeSelection(string Code, int? ConditionOn, bool Inversed
        , bool NullDefault = true, bool UseExistance = false)
    {
        public bool ShouldAppend(string?[] captureResult)
        {
			if (ConditionOn == null)
			{
				return !Inversed;
			}
            bool isTrue;
			if (!UseExistance)
            {
                string captured = NullDefault.ToString();
                if (ConditionOn.Value<captureResult.Length && ConditionOn.Value > 0)
                {
                    captured = captureResult[ConditionOn.Value] ?? captured;
                }
                isTrue = captured.ToLower().Trim() == "true";
            }
            else
            {
                string captured = string.Empty;
                if (ConditionOn.Value < captureResult.Length && ConditionOn.Value > 0)
                {
                    captured = captureResult[ConditionOn.Value] ?? captured;
                }
                isTrue = !string.IsNullOrWhiteSpace(captured);
            }
			return isTrue ^ Inversed;
		}
	}
}
